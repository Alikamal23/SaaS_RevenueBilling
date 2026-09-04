using RevenueBillingApi.Context;
using RevenueBillingApi.Extensions;
using RevenueBillingApi.Models.Authentication;
using RevenueBillingApi.Models.DMS;
using RevenueBillingApi.Models.MasterSetup;
using RevenueBillingApi.Models.Settings;
using RevenueBillingApi.RateLimiting;
using RevenueBillingApi.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace RevenueBillingApi.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/{controller}/{action}/{id:int?}")]
    [ApiController]
    public class MasterSetupController : ControllerBase
    {
        private readonly DataAccessLayer _DAL;
        private readonly SendEmail _sendemail;
        private readonly ILogger<MasterSetupController> _logger;
        private readonly DataEncryptor _dataencryptor;
        private readonly RandomStringGenerator _randomstringgenerator;
        private readonly CommonMethods _CommonMethods;

        public MasterSetupController(DataAccessLayer DAL, ILogger<MasterSetupController> logger, SendEmail sendemail, DataEncryptor dataencryptor, RandomStringGenerator randomstringgenerator, CommonMethods commonMethods)
        {
            _DAL = DAL;
            _logger = logger;
            _sendemail = sendemail;
            _dataencryptor = dataencryptor;
            _randomstringgenerator = randomstringgenerator;
            _CommonMethods = commonMethods;
        }

        #region Activity Log
        public void SystemActivityLog(int? ActivityID, string? ActivityDetails)
        {
            bool Result = false;

            ClaimsPrincipal claimsPrincipal = HttpContext.User;
            string HostName = Dns.GetHostName();
            IPHostEntry HostIPs = Dns.GetHostEntry(HostName);
            string IPAddress = HostIPs.AddressList[0].ToString();
            string UserID = (from c in claimsPrincipal.Claims where c.Type == "UserID" select c.Value).FirstOrDefault();
            var routeData = HttpContext.Request.RouteValues;
            string controllerName = routeData["controller"].ToString();
            string actionName = routeData["action"].ToString();
            string FormName = controllerName + "/" + actionName;
            string ActivityDetailsComplete = IPAddress + " " + ActivityDetails + " " + FormName;

            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("FormID-INT", "0");
                nv.Add("ActivityID-INT", ActivityID.ToString());
                nv.Add("UserID-INT", UserID);
                nv.Add("ActivityDetails-VARCHAR", ActivityDetailsComplete);
                Result = _DAL.InsertData("sp_insert_activitylog", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                _logger.LogInformation("{0} {1} {2}", controllerName, MethodBase.GetCurrentMethod().Name, ActivityDetailsComplete);
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", controllerName, MethodBase.GetCurrentMethod().Name, ActivityDetailsComplete, ex.Message);
            }
        }
        #endregion

        #region Master Setup

        #region Currency

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetCurrency()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getcurrency", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getcurrency");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveCurrency([FromBody] CurrencyMaster obj)
        {
            DataTable dt = null;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("CurrencyId-INT", obj.CurrencyId.ToString());
                nvCheck.Add("CurrencyDesc-VARCHAR", string.IsNullOrEmpty(obj.CurrencyDesc) ? "NULL" : obj.CurrencyDesc);

                var dtDup = _DAL.GetData("sp_checkduplicatecurrencyname", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection nv = new NameValueCollection();
                nv.Add("@CurrencyId-INT", obj.CurrencyId?.ToString() ?? "0");
                nv.Add("@CurrencyCode-NVARCHAR", obj.CurrencyCode ?? "");
                nv.Add("@CurrencyDesc-NVARCHAR", obj.CurrencyDesc ?? "");
                nv.Add("@CurrencyAbbr-NVARCHAR", obj.CurrencyAbbr ?? "");
                nv.Add("@CurrencyRate-FLOAT", obj.CurrencyRate.ToString());
                nv.Add("@DefaultCurrency-BIT", obj.DefaultCurrency.HasValue && obj.DefaultCurrency.Value ? "1" : "0");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId?.ToString() ?? "1");

                dt = _DAL.GetData("sp_savecurrency", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_savecurrency");
                    return Ok(dt);
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_savecurrency");
                    return BadRequest("No rows affected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditCurrency([FromBody] CurrencyMaster obj)
        {
            bool Result = false;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("CurrencyId-INT", obj.CurrencyId.ToString());
                nvCheck.Add("CurrencyDesc-VARCHAR", string.IsNullOrEmpty(obj.CurrencyDesc) ? "NULL" : obj.CurrencyDesc);

                var dtDup = _DAL.GetData("sp_checkduplicatecurrencyname", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("@CurrencyId-INT", obj.CurrencyId?.ToString() ?? "0");
                nv.Add("@CurrencyCode-NVARCHAR", obj.CurrencyCode ?? "");
                nv.Add("@CurrencyDesc-NVARCHAR", obj.CurrencyDesc ?? "");
                nv.Add("@CurrencyAbbr-NVARCHAR", obj.CurrencyAbbr ?? "");
                nv.Add("@CurrencyRate-FLOAT", obj.CurrencyRate.ToString());
                nv.Add("@DefaultCurrency-BIT", obj.DefaultCurrency.HasValue && obj.DefaultCurrency.Value ? "1" : "0");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId?.ToString() ?? "1");


                Result = _DAL.InsertData("sp_savecurrency", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_savecurrency");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_savecurrency");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteCurrency([FromBody] CurrencyMaster obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.CurrencyId == null ? "0" : obj.CurrencyId.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deletecurrency", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deletecurrency");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            return Ok(Result);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetCurrencyDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getcurrency_ddl", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getcurrency_ddl");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region ConversionRate

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetConversionRate()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getconversionrate", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getconversionrate");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveConversionRate([FromBody] ConversionRate obj)
        {
            DataTable dt = null;
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("@Id-INT", obj.id?.ToString() ?? "0");
                nv.Add("@CurrencyId-INT", obj.currency_id?.ToString() ?? "0");
                nv.Add("@ConversionDate-DATETIME", obj.conversion_date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                nv.Add("@ConversionRate-FLOAT", obj.conversion_rate?.ToString() ?? "0");
                nv.Add("@InstanceId-INT", obj.instanceid?.ToString() ?? "0");
                nv.Add("@CreatedBy-INT", obj.createdby ?? "0");
                //nv.Add("@CreatedOn-DATETIME", obj.createdon?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                //nv.Add("@UpdatedBy-INT", obj.updatedby ?? "0");
                //nv.Add("@UpdatedOn-DATETIME", obj.updatedon?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");

                dt = _DAL.GetData("sp_saveconversionrate", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_saveconversionrate");
                    return Ok(dt);
                }

                return BadRequest("No rows returned.");

            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditConversionRate([FromBody] ConversionRate obj)
        {
            //bool Result = false;
            DataTable dt = null;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("@Id-INT", obj.id?.ToString() ?? "0");
                nv.Add("@CurrencyId-INT", obj.currency_id?.ToString() ?? "0");
                nv.Add("@ConversionDate-DATETIME", obj.conversion_date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                nv.Add("@ConversionRate-FLOAT", obj.conversion_rate?.ToString() ?? "0");
                nv.Add("@InstanceId-INT", obj.instanceid?.ToString() ?? "0");
                nv.Add("@CreatedBy-INT", obj.createdby ?? "0");
                //nv.Add("@CreatedOn-DATETIME", obj.createdon?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                //nv.Add("@UpdatedBy-INT", obj.updatedby ?? "0");
                //nv.Add("@UpdatedOn-DATETIME", obj.updatedon?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");

                //Result = _DAL.InsertData("sp_saveconversionrate", nv, _DAL.CSManagementPortalDatabase);
                dt = _DAL.GetData("sp_saveconversionrate", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                //if (Result)
                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_saveconversionrate");
                    return Ok(dt);
                }

                return BadRequest("No rows returned.");

            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }

            //if (Result)
            //{
            //    return Ok(Result);
            //}
            //else
            //{
            //    return BadRequest(Result);
            //}
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteConversionRate([FromBody] ConversionRate obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.id == null ? "0" : obj.id.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deleteconversionrate", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deleteconversionrate");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }

            return Ok(Result);
        }


        #endregion


        #region VoucherType

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetVoucherType()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getvouchertype", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getvouchertype");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region JobOrder

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetJobOrder()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getjoborder", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getjoborder");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveJobOrder([FromBody] JobOrder obj)
        {
            DataTable dt = null;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("JobOrderId-INT", obj.JobOrderId.ToString());
                nvCheck.Add("JobNo-VARCHAR", string.IsNullOrEmpty(obj.JobNo) ? "NULL" : obj.JobNo);

                var dtDup = _DAL.GetData("sp_checkduplicatejobno", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection nv = new NameValueCollection();
                nv.Add("@JobOrderId-INT", obj.JobOrderId?.ToString() ?? "0");
                nv.Add("@JobNo-NVARCHAR", obj.JobNo ?? "");
                nv.Add("@JobDesc-NVARCHAR", obj.JobDesc ?? "");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId?.ToString() ?? "1");

                dt = _DAL.GetData("sp_saveJobOrder", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_saveJobOrder");
                    return Ok(dt);
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_saveJobOrder");
                    return BadRequest("No rows affected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditJobOrder([FromBody] JobOrder obj)
        {
            bool Result = false;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("JobOrderId-INT", obj.JobOrderId.ToString());
                nvCheck.Add("JobNo-VARCHAR", string.IsNullOrEmpty(obj.JobNo) ? "NULL" : obj.JobNo);

                var dtDup = _DAL.GetData("sp_checkduplicatejobno", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("@JobOrderId-INT", obj.JobOrderId?.ToString() ?? "0");
                nv.Add("@JobNo-NVARCHAR", obj.JobNo ?? "");
                nv.Add("@JobDesc-NVARCHAR", obj.JobDesc ?? "");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId?.ToString() ?? "1");


                Result = _DAL.InsertData("sp_saveJobOrder", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_saveJobOrder");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_saveJobOrder");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteJobOrder([FromBody] JobOrder obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.JobOrderId == null ? "0" : obj.JobOrderId.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deletejoborder", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deletejoborder");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            return Ok(Result);
        }

        #endregion

        #region Chart of Account

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetFillFinancialCategoryDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("proc_WF_GetFillFinancialCategoryDDL", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "proc_WF_GetFillFinancialCategoryDDL");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }


        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetAutoGLCode(string Level, string FC)
        {

            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("Level-NVARCHAR", Level);
                nv.Add("FC-NVARCHAR", FC);
                dt = _DAL.GetData("proc_WF_GetGLCode", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "proc_WF_GetGLCode");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }


        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetFillDropdown(string Level, string FC)
        {

            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("Level-NVARCHAR", Level);
                nv.Add("FC-NVARCHAR", FC);
                dt = _DAL.GetData("proc_WF_GetFillDropdown", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "proc_WF_GetFillDropdown");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetFillCategory(string accountid)
        {

            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("FK_AccountId-INT", accountid);
                dt = _DAL.GetData("proc_WF_GetCategory", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "proc_WF_GetCategory");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveDataLevel2([FromBody] tbl_Account_Level2 obj)
        {
            DataTable dt = null;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("Id-INT", obj.Id.ToString());
                nvCheck.Add("Name-VARCHAR", string.IsNullOrEmpty(obj.Name) ? "NULL" : obj.Name);

                var dtDup = _DAL.GetData("sp_checkduplicatelevel2", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection nv = new NameValueCollection();
                nv.Add("@Id-INT", obj.Id.ToString() ?? "0");
                nv.Add("@Name-NVARCHAR", obj.Name ?? "");
                nv.Add("@GLCode-NVARCHAR", obj.GLCode ?? "");
                nv.Add("@FC-NVARCHAR", obj.FC ?? "");
                nv.Add("@FC_Nature-NVARCHAR", obj.FC_Nature ?? "");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId?.ToString() ?? "1");

                dt = _DAL.GetData("sp_saveL2Account", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_saveL2Account");
                    return Ok(dt);
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_saveL2Account");
                    return BadRequest("No rows affected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveDataLevel3([FromBody] tbl_Account_Level3 obj)
        {
            DataTable dt = null;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("Id-INT", obj.Id.ToString());
                nvCheck.Add("Name-VARCHAR", string.IsNullOrEmpty(obj.Name) ? "NULL" : obj.Name);

                var dtDup = _DAL.GetData("sp_checkduplicatelevel3", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection nv = new NameValueCollection();
                nv.Add("@Id-INT", obj.Id.ToString() ?? "0");
                nv.Add("@Name-NVARCHAR", obj.Name ?? "");
                nv.Add("@GLCode-NVARCHAR", obj.GLCode ?? "");
                nv.Add("@FK_AccountId-INT", obj.FK_AccountId?.ToString() ?? "0");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId?.ToString() ?? "1");

                dt = _DAL.GetData("sp_saveL3Category", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_saveL3Category");
                    return Ok(dt);
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_saveL3Category");
                    return BadRequest("No rows affected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }


        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveDataLevel4([FromBody] tbl_Account_Level4 obj)
        {
            DataTable dt = null;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("Id-INT", obj.Id.ToString());
                nvCheck.Add("Name-VARCHAR", string.IsNullOrEmpty(obj.Name) ? "NULL" : obj.Name);

                var dtDup = _DAL.GetData("sp_checkduplicatelevel4", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection nv = new NameValueCollection();
                nv.Add("@Id-INT", obj.Id.ToString() ?? "0");
                nv.Add("@Name-NVARCHAR", obj.Name ?? "");
                nv.Add("@GLCode-NVARCHAR", obj.GLCode ?? "");
                nv.Add("@CB-NVARCHAR", obj.CB ?? "");                     // New field for Level 4
                nv.Add("@FK_CategoryId-INT", obj.FK_CategoryId?.ToString() ?? "0");  // Foreign key to Level 3
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId?.ToString() ?? "1");

                dt = _DAL.GetData("sp_saveL4CategoryHead", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_saveL4CategoryHead");
                    return Ok(dt);
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_saveL4CategoryHead");
                    return BadRequest("No rows affected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }


        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetChartofAccount()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getchartofaccount", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getchartofaccount");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }



        #endregion

        #region WarehouseMaster
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWarehouse()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getwarehouse", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getwarehouse");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveWarehouse([FromBody] WarehouseMaster obj)
        {
            DataTable dt = null;
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("WarehouseId-INT", obj.WarehouseId.ToString());
                nv.Add("WarehouseDesc-VARCHAR", string.IsNullOrEmpty(obj.WarehouseDesc) ? "NULL" : obj.WarehouseDesc);
                nv.Add("Address-VARCHAR", string.IsNullOrEmpty(obj.Address) ? "NULL" : obj.Address);
                nv.Add("IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");

                dt = _DAL.GetData("sp_savewarehouse", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_savewarehouse");
                    return Ok(dt);
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_savewarehouse");
                    return BadRequest("No rows affected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditWarehouse([FromBody] WarehouseMaster obj)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("WarehouseId-INT", obj.WarehouseId.ToString());
                nv.Add("WarehouseDesc-VARCHAR", string.IsNullOrEmpty(obj.WarehouseDesc) ? "NULL" : obj.WarehouseDesc);
                nv.Add("Address-VARCHAR", string.IsNullOrEmpty(obj.Address) ? "NULL" : obj.Address);
                nv.Add("IsActive-BIT", obj.IsActive == null ? "false" : obj.IsActive.ToString());
                Result = _DAL.InsertData("sp_savewarehouse", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_savewarehouse");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_savewarehouse");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteWarehouse([FromBody] WarehouseMaster obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("WarehouseId-INT", obj.WarehouseId == null ? "0" : obj.WarehouseId.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deletewarehouse", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deletewarehouse");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            return Ok(Result);
        }

        #endregion

        #region Manufacturer

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetManufacturer()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getmanufacturer", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getmanufacturer");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveManufacturer([FromBody] ManufacturerMaster obj)
        {
            DataTable dt = null;
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("Id-INT", obj.Id.ToString());
                nv.Add("Name-VARCHAR", string.IsNullOrEmpty(obj.Name) ? "NULL" : obj.Name);
                nv.Add("IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");

                dt = _DAL.GetData("sp_savemanufacturer", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_savemanufacturer");
                    return Ok(dt);
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_savemanufacturer");
                    return BadRequest("No rows affected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditManufacturer([FromBody] ManufacturerMaster obj)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.Id.ToString());
                nv.Add("Name-VARCHAR", string.IsNullOrEmpty(obj.Name) ? "NULL" : obj.Name);
                nv.Add("IsActive-BIT", obj.IsActive == null ? "false" : obj.IsActive.ToString());
                Result = _DAL.InsertData("sp_savemanufacturer", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_savemanufacturer");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_savemanufacturer");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteManufacturer([FromBody] ManufacturerMaster obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.Id == null ? "0" : obj.Id.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deletemanufacturer", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deletemanufacturer");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            return Ok(Result);
        }

        #endregion

        #region Product

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetProduct()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getproduct", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getproduct");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveProduct([FromBody] ProductMaster obj)
        {
            DataTable dt = null;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("ProductId-INT", obj.PartyId.ToString());
                nvCheck.Add("ProductName-VARCHAR", string.IsNullOrEmpty(obj.ProductName) ? "NULL" : obj.ProductName);

                var dtDup = _DAL.GetData("sp_checkduplicateproductname", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection nv = new NameValueCollection();
                nv.Add("@ProductId-INT", obj.ProductId?.ToString() ?? "0");
                nv.Add("@ProductName-NVARCHAR", obj.ProductName ?? "");
                nv.Add("@Category-NVARCHAR", obj.Category ?? "");
                nv.Add("@Specification-NVARCHAR", obj.Specification ?? "");
                nv.Add("@Unit-NVARCHAR", obj.Unit ?? "");
                nv.Add("@CP-FLOAT", obj.CP.ToString());
                nv.Add("@SP-FLOAT", obj.SP.ToString());
                nv.Add("@ReOrderQty-INT", obj.ReOrderQty?.ToString() ?? "0");
                nv.Add("@Packing-FLOAT", obj.Packing.ToString());
                nv.Add("@ProductType-NVARCHAR", obj.ProductType ?? "");
                nv.Add("@PartyId-INT", obj.PartyId?.ToString() ?? "0");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId?.ToString() ?? "1");

                dt = _DAL.GetData("sp_saveproduct", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_saveproduct");
                    return Ok(dt);
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_saveproduct");
                    return BadRequest("No rows affected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditProduct([FromBody] ProductMaster obj)
        {
            bool Result = false;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("ProductId-INT", obj.PartyId.ToString());
                nvCheck.Add("ProductName-VARCHAR", string.IsNullOrEmpty(obj.ProductName) ? "NULL" : obj.ProductName);

                var dtDup = _DAL.GetData("sp_checkduplicateproductname", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("@ProductId-INT", obj.ProductId?.ToString() ?? "0");
                nv.Add("@ProductName-NVARCHAR", obj.ProductName ?? "");
                nv.Add("@Category-NVARCHAR", obj.Category ?? "");
                nv.Add("@Specification-NVARCHAR", obj.Specification ?? "");
                nv.Add("@Unit-NVARCHAR", obj.Unit ?? "");
                nv.Add("@CP-FLOAT", obj.CP.ToString());
                nv.Add("@SP-FLOAT", obj.SP.ToString());
                nv.Add("@ReOrderQty-INT", obj.ReOrderQty?.ToString() ?? "0");
                nv.Add("@Packing-FLOAT", obj.Packing.ToString());
                nv.Add("@ProductType-NVARCHAR", obj.ProductType ?? "");
                nv.Add("@PartyId-INT", obj.PartyId?.ToString() ?? "0");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId?.ToString() ?? "1");

                Result = _DAL.InsertData("sp_saveproduct", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_saveproduct");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_saveproduct");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteProduct([FromBody] ProductMaster obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.ProductId == null ? "0" : obj.ProductId.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deleteproduct", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deleteproduct");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            return Ok(Result);
        }

        #endregion 

        #region Party / Customer or Supplier

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetParty(string CategoryCode)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getparty", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getparty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveParty([FromBody] Party obj)
        {
            DataTable dt = null;
            try
            {

                // ✅ Get Max PartyCode Category wise
                NameValueCollection nvMaxCode = new NameValueCollection();
                nvMaxCode.Add("CategoryCode-INT", obj.CategoryCode.ToString());

                var dtMax = _DAL.GetData("sp_getpartycode", nvMaxCode, _DAL.CSManagementPortalDatabase);

                if (dtMax.Rows.Count > 0 && Convert.ToInt32(dtMax.Rows[0]["MAX_CODE"]) > 0)
                {
                    obj.PartyCode = Convert.ToInt32(dtMax.Rows[0]["MAX_CODE"]);
                }


                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("PartyId-INT", obj.PartyId.ToString());
                nvCheck.Add("PartyName-VARCHAR", string.IsNullOrEmpty(obj.PartyName) ? "NULL" : obj.PartyName);

                var dtDup = _DAL.GetData("sp_checkduplicatepartyname", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection nv = new NameValueCollection();
                nv.Add("@PartyId-INT", obj.PartyId.HasValue ? obj.PartyId.Value.ToString() : "0");
                nv.Add("@PartyCode-INT", obj.PartyCode.HasValue ? obj.PartyCode.Value.ToString() : "0");
                nv.Add("@PartyName-NVARCHAR", obj.PartyName ?? "");
                nv.Add("@Contact-NVARCHAR", obj.Contact ?? "");
                nv.Add("@Address-NVARCHAR", obj.Address ?? "");
                nv.Add("@Phone1-NVARCHAR", obj.Phone1 ?? "");
                nv.Add("@Phone2-NVARCHAR", obj.Phone2 ?? "");
                nv.Add("@Cell-NVARCHAR", obj.Cell ?? "");
                nv.Add("@Fax-NVARCHAR", obj.Fax ?? "");
                nv.Add("@Email-NVARCHAR", obj.Email ?? "");
                nv.Add("@City-NVARCHAR", obj.City ?? "");
                nv.Add("@PartyGroup-NVARCHAR", obj.PartyGroup ?? "");
                nv.Add("@Remarks-NVARCHAR", obj.Remarks ?? "");
                nv.Add("@Commission-FLOAT", obj.Commission.ToString());
                nv.Add("@VanSharing-FLOAT", obj.VanSharing.ToString());
                nv.Add("@CategoryCode-INT", obj.CategoryCode.HasValue ? obj.CategoryCode.Value.ToString() : "0");
                nv.Add("@GLCode-NVARCHAR", obj.GLCode ?? "");
                nv.Add("@NTNNo-NVARCHAR", obj.NTNNo ?? "");
                nv.Add("@GSTNo-NVARCHAR", obj.GSTNo ?? "");
                nv.Add("@GST-FLOAT", obj.GST.ToString());
                nv.Add("@SST-FLOAT", obj.SST.ToString());
                nv.Add("@WHT-FLOAT", obj.WHT.ToString());
                nv.Add("@OpeningBalance-FLOAT", obj.OpeningBalance.ToString());
                nv.Add("@OpeningDate-DATETIME", obj.OpeningDate.HasValue ? obj.OpeningDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                nv.Add("@PaymentTerm-NVARCHAR", obj.PaymentTerm ?? "");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId.HasValue ? obj.BranchId.Value.ToString() : "1");

                dt = _DAL.GetData("sp_saveparty", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_saveparty");
                    return Ok(dt);
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_saveparty");
                    return BadRequest("No rows affected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditParty([FromBody] Party obj)
        {
            bool Result = false;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("PartyId-INT", obj.PartyId.ToString());
                nvCheck.Add("PartyName-VARCHAR", string.IsNullOrEmpty(obj.PartyName) ? "NULL" : obj.PartyName);

                var dtDup = _DAL.GetData("sp_checkduplicatepartyname", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("@PartyId-INT", obj.PartyId.HasValue ? obj.PartyId.Value.ToString() : "0");
                nv.Add("@PartyCode-INT", obj.PartyCode.HasValue ? obj.PartyCode.Value.ToString() : "0");
                nv.Add("@PartyName-NVARCHAR", obj.PartyName ?? "");
                nv.Add("@Contact-NVARCHAR", obj.Contact ?? "");
                nv.Add("@Address-NVARCHAR", obj.Address ?? "");
                nv.Add("@Phone1-NVARCHAR", obj.Phone1 ?? "");
                nv.Add("@Phone2-NVARCHAR", obj.Phone2 ?? "");
                nv.Add("@Cell-NVARCHAR", obj.Cell ?? "");
                nv.Add("@Fax-NVARCHAR", obj.Fax ?? "");
                nv.Add("@Email-NVARCHAR", obj.Email ?? "");
                nv.Add("@City-NVARCHAR", obj.City ?? "");
                nv.Add("@PartyGroup-NVARCHAR", obj.PartyGroup ?? "");
                nv.Add("@Remarks-NVARCHAR", obj.Remarks ?? "");
                nv.Add("@Commission-FLOAT", obj.Commission.ToString());
                nv.Add("@VanSharing-FLOAT", obj.VanSharing.ToString());
                nv.Add("@CategoryCode-INT", obj.CategoryCode.HasValue ? obj.CategoryCode.Value.ToString() : "0");
                nv.Add("@GLCode-NVARCHAR", obj.GLCode ?? "");
                nv.Add("@NTNNo-NVARCHAR", obj.NTNNo ?? "");
                nv.Add("@GSTNo-NVARCHAR", obj.GSTNo ?? "");
                nv.Add("@GST-FLOAT", obj.GST.ToString());
                nv.Add("@SST-FLOAT", obj.SST.ToString());
                nv.Add("@WHT-FLOAT", obj.WHT.ToString());
                nv.Add("@OpeningBalance-FLOAT", obj.OpeningBalance.ToString());
                nv.Add("@OpeningDate-DATETIME", obj.OpeningDate.HasValue ? obj.OpeningDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                nv.Add("@PaymentTerm-NVARCHAR", obj.PaymentTerm ?? "");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId.HasValue ? obj.BranchId.Value.ToString() : "1");

                Result = _DAL.InsertData("sp_saveparty", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_saveparty");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_saveparty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteParty([FromBody] Party obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PartyId == null ? "0" : obj.PartyId.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deleteparty", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deleteparty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            return Ok(Result);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetPartyDDL(string CategoryCode)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getparty", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getparty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWarehouseDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                //NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getwarehouseddl", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getwarehouseddl");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetProductDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                //NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getproductddl", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getproductddl");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region Get all Parent Company
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetCompanyDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getcompany", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getcompany");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }
        #endregion

        #endregion

        // Revenue & Billing Suite

        #region Revenue & Billing Suite

        #region Client Setup

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetClient()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getparty", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getparty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveClient([FromBody] Client obj)
        {
            DataTable dt = null;
            try
            {

                // ✅ Get Max PartyCode Category wise
                NameValueCollection nvMaxCode = new NameValueCollection();
                nvMaxCode.Add("CategoryCode-INT", obj.CategoryCode.ToString());

                var dtMax = _DAL.GetData("sp_getpartycode", nvMaxCode, _DAL.CSManagementPortalDatabase);

                if (dtMax.Rows.Count > 0 && Convert.ToInt32(dtMax.Rows[0]["MAX_CODE"]) > 0)
                {
                    obj.PartyCode = Convert.ToInt32(dtMax.Rows[0]["MAX_CODE"]);
                }


                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("PartyId-INT", obj.PartyId.ToString());
                nvCheck.Add("PartyName-VARCHAR", string.IsNullOrEmpty(obj.PartyName) ? "NULL" : obj.PartyName);

                var dtDup = _DAL.GetData("sp_checkduplicatepartyname", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection nv = new NameValueCollection();
                nv.Add("@PartyId-INT", obj.PartyId.HasValue ? obj.PartyId.Value.ToString() : "0");
                nv.Add("@PartyCode-INT", obj.PartyCode.HasValue ? obj.PartyCode.Value.ToString() : "0");
                nv.Add("@PartyName-NVARCHAR", obj.PartyName ?? "");
                nv.Add("@Contact-NVARCHAR", obj.Contact ?? "");
                nv.Add("@Address-NVARCHAR", obj.Address ?? "");
                nv.Add("@Phone1-NVARCHAR", obj.Phone1 ?? "");
                nv.Add("@Phone2-NVARCHAR", obj.Phone2 ?? "");
                nv.Add("@Cell-NVARCHAR", obj.Cell ?? "");
                nv.Add("@Fax-NVARCHAR", obj.Fax ?? "");
                nv.Add("@Email-NVARCHAR", obj.Email ?? "");
                nv.Add("@City-NVARCHAR", obj.City ?? "");
                nv.Add("@PartyGroup-NVARCHAR", obj.PartyGroup ?? "");
                nv.Add("@Remarks-NVARCHAR", obj.Remarks ?? "");
                nv.Add("@Commission-FLOAT", obj.Commission.ToString());
                nv.Add("@VanSharing-FLOAT", obj.VanSharing.ToString());
                nv.Add("@CategoryCode-INT", obj.CategoryCode.HasValue ? obj.CategoryCode.Value.ToString() : "0");
                nv.Add("@GLCode-NVARCHAR", obj.GLCode ?? "");
                nv.Add("@NTNNo-NVARCHAR", obj.NTNNo ?? "");
                nv.Add("@GSTNo-NVARCHAR", obj.GSTNo ?? "");
                nv.Add("@GST-FLOAT", obj.GST.ToString());
                nv.Add("@SST-FLOAT", obj.SST.ToString());
                nv.Add("@WHT-FLOAT", obj.WHT.ToString());
                nv.Add("@OpeningBalance-FLOAT", obj.OpeningBalance.ToString());
                nv.Add("@OpeningDate-DATETIME", obj.OpeningDate.HasValue ? obj.OpeningDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                nv.Add("@PaymentTerm-NVARCHAR", obj.PaymentTerm ?? "");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId.HasValue ? obj.BranchId.Value.ToString() : "1");

                dt = _DAL.GetData("sp_saveparty", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_saveparty");
                    return Ok(dt);
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_saveparty");
                    return BadRequest("No rows affected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditClient([FromBody] Client obj)
        {
            bool Result = false;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("PartyId-INT", obj.PartyId.ToString());
                nvCheck.Add("PartyName-VARCHAR", string.IsNullOrEmpty(obj.PartyName) ? "NULL" : obj.PartyName);

                var dtDup = _DAL.GetData("sp_checkduplicatepartyname", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameRecord" });
                }


                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("@PartyId-INT", obj.PartyId.HasValue ? obj.PartyId.Value.ToString() : "0");
                nv.Add("@PartyCode-INT", obj.PartyCode.HasValue ? obj.PartyCode.Value.ToString() : "0");
                nv.Add("@PartyName-NVARCHAR", obj.PartyName ?? "");
                nv.Add("@Contact-NVARCHAR", obj.Contact ?? "");
                nv.Add("@Address-NVARCHAR", obj.Address ?? "");
                nv.Add("@Phone1-NVARCHAR", obj.Phone1 ?? "");
                nv.Add("@Phone2-NVARCHAR", obj.Phone2 ?? "");
                nv.Add("@Cell-NVARCHAR", obj.Cell ?? "");
                nv.Add("@Fax-NVARCHAR", obj.Fax ?? "");
                nv.Add("@Email-NVARCHAR", obj.Email ?? "");
                nv.Add("@City-NVARCHAR", obj.City ?? "");
                nv.Add("@PartyGroup-NVARCHAR", obj.PartyGroup ?? "");
                nv.Add("@Remarks-NVARCHAR", obj.Remarks ?? "");
                nv.Add("@Commission-FLOAT", obj.Commission.ToString());
                nv.Add("@VanSharing-FLOAT", obj.VanSharing.ToString());
                nv.Add("@CategoryCode-INT", obj.CategoryCode.HasValue ? obj.CategoryCode.Value.ToString() : "0");
                nv.Add("@GLCode-NVARCHAR", obj.GLCode ?? "");
                nv.Add("@NTNNo-NVARCHAR", obj.NTNNo ?? "");
                nv.Add("@GSTNo-NVARCHAR", obj.GSTNo ?? "");
                nv.Add("@GST-FLOAT", obj.GST.ToString());
                nv.Add("@SST-FLOAT", obj.SST.ToString());
                nv.Add("@WHT-FLOAT", obj.WHT.ToString());
                nv.Add("@OpeningBalance-FLOAT", obj.OpeningBalance.ToString());
                nv.Add("@OpeningDate-DATETIME", obj.OpeningDate.HasValue ? obj.OpeningDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                nv.Add("@PaymentTerm-NVARCHAR", obj.PaymentTerm ?? "");
                nv.Add("@IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.EditID ?? "superadmin");
                nv.Add("@BranchId-INT", obj.BranchId.HasValue ? obj.BranchId.Value.ToString() : "1");

                Result = _DAL.InsertData("sp_saveparty", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_saveparty");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_saveparty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteClient([FromBody] Client obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PartyId == null ? "0" : obj.PartyId.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deleteparty", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deleteparty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            return Ok(Result);
        }

        #endregion

        #region Populate DDLs

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetClientDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getparty", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getparty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetIndustryDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getindustry", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getindustry");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetCompanySizeDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getcompanysize", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getcompanysize");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetCountryDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getcountry", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getcountry");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetContractTypeDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getcontracttype", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getcontracttype");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetAccountOwnerDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getaccountowner", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getaccountowner");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetSupportOwnerDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getsupportowner", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getsupportowner");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetContractTypeBasicDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getcontracttypebasic", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getcontracttypebasic");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetBillingFrequencyDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getbillingfrequency", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getbillingfrequency");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetPaymentTermsDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getpaymentterms", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getpaymentterms");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetBillingTypeDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getbillingtype", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getbillingtype");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetBillingStatusDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getbillingstatus", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getbillingstatus");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetClientInfoDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getclientinfo_ddl", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getclientinfo_ddl");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetContractInfoDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getcontractinfo_ddl", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getcontractinfo_ddl");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetRecurringBillingDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getrecurringbillingddl", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getrecurringbillingddl");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetMilestoneDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getmilestoneddl", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getmilestoneddl");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetInvoiceDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getallinvoiceddl", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getallinvoiceddl");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "MasterSetupController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }


        #endregion


        #endregion


    }
}
