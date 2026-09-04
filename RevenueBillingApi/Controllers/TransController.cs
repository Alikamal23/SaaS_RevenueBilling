using RevenueBillingApi.Context;
using RevenueBillingApi.Extensions;
using RevenueBillingApi.Models.Authentication;
using RevenueBillingApi.Models.COB;
using RevenueBillingApi.Models.DMS;
using RevenueBillingApi.Models.MasterSetup;
using RevenueBillingApi.Models.Settings;
using RevenueBillingApi.RateLimiting;
using RevenueBillingApi.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.DirectoryServices;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace RevenueBillingApi.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/{controller}/{action}/{id:int?}")]
    [ApiController]
    public class TransController : ControllerBase
    {
        private readonly DataAccessLayer _DAL;
        private readonly SendEmail _sendemail;
        private readonly ILogger<TransController> _logger;
        private readonly DataEncryptor _dataencryptor;
        private readonly RandomStringGenerator _randomstringgenerator;
        private readonly CommonMethods _CommonMethods;

        public TransController(DataAccessLayer DAL, ILogger<TransController> logger, SendEmail sendemail, DataEncryptor dataencryptor, RandomStringGenerator randomstringgenerator, CommonMethods commonMethods)
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

        #region ERP -> Transaction

        #region Account_VoucherSystem

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

        #endregion

        #region GetVoucherTypeDDL
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetVoucherTypeDDL(string nature)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("Nature-VARCHAR", string.IsNullOrEmpty(nature) ? "-1" : nature);

                dt = _DAL.GetData("proc_WF_GetVoucherTypeDDL", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "proc_WF_GetVoucherTypeDDL");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region GetMaxVoucherID
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetMaxVoucherID(string vtype)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("VType-VARCHAR", string.IsNullOrEmpty(vtype) ? "NULL" : vtype);

                dt = _DAL.GetData("proc_WF_GetMaxVoucherID", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "proc_WF_GetMaxVoucherID");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region GetHeadDDL
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetHeadDDL(string cb, string headType)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("CB-VARCHAR", string.IsNullOrEmpty(cb) ? "NULL" : cb);
                nv.Add("HeadType-VARCHAR", string.IsNullOrEmpty(headType) ? "NULL" : headType);

                dt = _DAL.GetData("proc_WF_GetHeadDDL", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "proc_WF_GetHeadDDL");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region GetJobDDL
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetJobDDL()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_getalljob", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getalljob");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion


        // Voucher Save and Get Voucher Grid

        #region Get Voucher Grid
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetVoucherMaster(string Nature)
        {
            DataSet ds = new DataSet();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("Nature-VARCHAR", string.IsNullOrEmpty(Nature) ? "NULL" : Nature);

                ds = _DAL.GetDataSet("GetVoucherMaster", nv, _DAL.CSManagementPortalDatabase);

                if (ds == null)
                    return BadRequest("SP returned no data");


                if (ds != null && ds.Tables.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getalljob");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            //return Ok(ds);
            return Ok(new
            {
                Master = ds.Tables[0],
                Detail = ds.Tables.Count > 1 ? ds.Tables[1] : null
            });

        }
        #endregion

        #region Save Voucher
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveVoucher([FromBody] tbl_Voucher obj)
        {
            DataTable dt = null;
            DataTable dtD = null;
             
            try
            {
                // ------------------------------
                // 1. Insert Voucher Header
                // ------------------------------
                NameValueCollection nv = new NameValueCollection();
                nv.Add("@Id-INT", obj.VoucherId.ToString());
                nv.Add("@VType-NVARCHAR", obj.VType ?? "");
                nv.Add("@VCode-INT", obj.VCode.ToString());
                nv.Add("@VDate-DATETIME", obj.VDate?.ToString("yyyy-MM-dd"));
                nv.Add("@VNo-NVARCHAR", obj.VNo ?? "");
                nv.Add("@Folio-NVARCHAR", obj.Folio ?? "");
                nv.Add("@ModeOfPayment-NVARCHAR", obj.ModeOfPayment ?? "");
                nv.Add("@BankName-NVARCHAR", obj.BankName ?? "");
                nv.Add("@Chq_PO_Draft_No-NVARCHAR", obj.Chq_PO_Draft_No ?? "");
                nv.Add("@CashType-NVARCHAR", obj.CashType ?? "");
                nv.Add("@Narration-NVARCHAR", obj.Narration ?? "");
                nv.Add("@DCCategory-INT", obj.DCCategory.ToString());
                nv.Add("@DCCode-INT", obj.DCCode.ToString());
                nv.Add("@Amount-FLOAT", obj.Amount.ToString());
                nv.Add("@FK_InstanceID-INT", "0");
                nv.Add("@CreatedBy-NVARCHAR", obj.CreatedBy ?? "0");
                nv.Add("@UpdatedBy-NVARCHAR", "0");

                dt = _DAL.GetData("sp_voucher", nv, _DAL.CSManagementPortalDatabase);

                // Validate header insert result
                if (dt == null || dt.Rows.Count == 0)
                {
                    return BadRequest("Voucher insertion failed (No rows returned).");
                }

                // Get new Voucher Id (returned by SP)
                int newId = Convert.ToInt32(dt.Rows[0]["Id"]);

                // If SP generates VNo, use it here
                string generatedVNo = ""; // dt.Columns.Contains("VNo") ? dt.Rows[0]["VNo"].ToString() : obj.VNo;


                // ------------------------------
                // 2. Insert Voucher Detail Rows
                // ------------------------------
                if (obj.DetailSection != null && obj.DetailSection.Count > 0)
                {
                    foreach (var ts in obj.DetailSection)
                    {
                        NameValueCollection nv1 = new NameValueCollection();
                        nv1.Add("@VType-NVARCHAR", obj.VType ?? "");
                        nv1.Add("@VCode-INT", obj.VCode.ToString());
                        nv1.Add("@VNo-NVARCHAR", generatedVNo);   
                        nv1.Add("@CategoryCode-INT", ts.CategoryCode.ToString());
                        nv1.Add("@HeadCode-INT", ts.HeadCode.ToString());
                        nv1.Add("@Remarks-NVARCHAR", ts.Remarks ?? "");
                        nv1.Add("@HeadType-NVARCHAR", ts.HeadType ?? "");
                        nv1.Add("@JobNo-NVARCHAR", ts.JobNo ?? "");
                        nv1.Add("@BillNo-NVARCHAR", ts.BillNo ?? "");
                        nv1.Add("@ChqNo-NVARCHAR", ts.ChqNo ?? "");
                        nv1.Add("@ChqDate-DATETIME", ts.ChqDate.HasValue ? ts.ChqDate.Value.ToString("yyyy-MM-dd") : "NULL");
                        nv1.Add("@ClearDate-DATETIME", ts.ClearDate.HasValue ? ts.ClearDate.Value.ToString("yyyy-MM-dd") : "NULL");
                        nv1.Add("@ReturnDate-DATETIME", ts.ReturnDate.HasValue ? ts.ReturnDate.Value.ToString("yyyy-MM-dd") : "NULL");
                        nv1.Add("@DebitAmount-FLOAT", ts.DebitAmount.ToString());
                        nv1.Add("@CreditAmount-FLOAT", ts.CreditAmount.ToString());
                        nv1.Add("@FK_VoucherID-INT", newId.ToString());

                        dtD = _DAL.GetData("sp_voucher_detail", nv1, _DAL.CSManagementPortalDatabase);
                    }
                }


                // ------------------------------
                // 3. Final Success Response
                // ------------------------------
                SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + " Add_WF_VoucherData");

                return Ok(new
                {
                    status = "success",
                    voucherId = newId,
                    vNo = generatedVNo,
                    data = dt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);

                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);

                return BadRequest("Something Went Wrong Please Contact Your System Administrator");
            }
        }
        #endregion

        #region Delete Voucher
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteVoucher([FromBody] tbl_Voucher obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.VoucherId == null ? "0" : obj.VoucherId.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("DeleteVoucher", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "DeleteVoucher");
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

        #region Edit Voucher
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditVoucher([FromBody] tbl_Voucher obj)
        {
            try
            {
                if (obj == null || obj.VoucherId <= 0)
                    return BadRequest("Invalid Voucher Id");

                // ------------------ UPDATE MASTER ------------------
                NameValueCollection nv = new NameValueCollection();
                nv.Add("@Id-INT", obj.VoucherId.ToString());
                nv.Add("@VType-NVARCHAR", obj.VType);
                nv.Add("@VCode-INT", obj.VCode.ToString());
                nv.Add("@VDate-DATETIME", obj.VDate.Value.ToString("yyyy-MM-dd"));
                nv.Add("@VNo-NVARCHAR", obj.VNo);
                nv.Add("@Folio-NVARCHAR", obj.Folio ?? "");
                nv.Add("@ModeOfPayment-NVARCHAR", obj.ModeOfPayment ?? "");
                nv.Add("@BankName-NVARCHAR", obj.BankName ?? "");
                nv.Add("@Chq_PO_Draft_No-NVARCHAR", obj.Chq_PO_Draft_No ?? "");
                nv.Add("@CashType-NVARCHAR", obj.CashType ?? "");
                nv.Add("@Narration-NVARCHAR", obj.Narration ?? "");
                nv.Add("@DCCategory-INT", obj.DCCategory.ToString());
                nv.Add("@DCCode-INT", obj.DCCode.ToString());
                nv.Add("@Amount-DECIMAL", obj.Amount.ToString());
                nv.Add("@UpdatedBy-NVARCHAR", obj.EditID);

                DataTable dt = _DAL.GetData("sp_EditVoucher", nv, _DAL.CSManagementPortalDatabase);
                if (dt == null)
                    return BadRequest("Master update failed");

                // ------------------ DELETE OLD DETAILS ------------------
                NameValueCollection nvDel = new NameValueCollection();
                nvDel.Add("@FK_VoucherID-INT", obj.VoucherId.ToString());
                _DAL.GetData("sp_deleteVoucherDetails", nvDel, _DAL.CSManagementPortalDatabase);

                // ------------------ INSERT NEW DETAILS ------------------
                foreach (var d in obj.DetailSection)
                {
                    NameValueCollection nd = new NameValueCollection();
                    nd.Add("@FK_VoucherID-INT", obj.VoucherId.ToString());
                    nd.Add("@VType-NVARCHAR", obj.VType);
                    nd.Add("@VCode-INT", obj.VCode.ToString());
                    nd.Add("@VNo-NVARCHAR", obj.VNo);
                    nd.Add("@CategoryCode-INT", d.CategoryCode.ToString());
                    nd.Add("@HeadCode-INT", d.HeadCode.ToString());
                    nd.Add("@Remarks-NVARCHAR", d.Remarks ?? "");
                    nd.Add("@HeadType-NVARCHAR", d.HeadType ?? "");
                    nd.Add("@JobNo-NVARCHAR", d.JobNo ?? "");
                    nd.Add("@BillNo-NVARCHAR", d.BillNo ?? "");
                    nd.Add("@ChqNo-NVARCHAR", d.ChqNo ?? "");
                    nd.Add("@ChqDate-DATETIME", d.ChqDate?.ToString("yyyy-MM-dd"));
                    nd.Add("@ClearDate-DATETIME", d.ClearDate?.ToString("yyyy-MM-dd"));
                    nd.Add("@ReturnDate-DATETIME", d.ReturnDate?.ToString("yyyy-MM-dd"));
                    nd.Add("@DebitAmount-DECIMAL", d.DebitAmount.ToString());
                    nd.Add("@CreditAmount-DECIMAL", d.CreditAmount.ToString());

                    _DAL.InsertData("sp_EditVoucher_Detail", nd, _DAL.CSManagementPortalDatabase);
                }

                return Ok(new { success = true, voucherId = obj.VoucherId });
            }
            catch (Exception ex)
            {
                _logger.LogError("EditVoucher Error: " + ex.Message);
                return BadRequest("Something went wrong");
            }
        }
        #endregion

        #endregion

        #region PurchaseOrder

        #region GetMaxID
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetMaxID(string table)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("tableName-VARCHAR", string.IsNullOrEmpty(table) ? "NULL" : table);

                dt = _DAL.GetData("proc_WF_GetMaxID", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "proc_WF_GetMaxID");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region Save / Update Purchase Order
        [HttpPost]
        public IActionResult SavePurchaseOrder([FromBody] Purchase_Order obj)
        {
            DataTable dt;

            try
            {
                // ---------------- MASTER (UPSERT) ----------------
                NameValueCollection nv = new NameValueCollection();
                nv.Add("@Id-INT", obj.Id.ToString());
                nv.Add("@POId-INT", obj.POId.ToString());   // update me actual POId, insert me 0
                nv.Add("@OrderDate-DATETIME", obj.OrderDate?.ToString("yyyy-MM-dd"));
                nv.Add("@RefNo-NVARCHAR", obj.RefNo ?? "");
                nv.Add("@PartyId-INT", obj.PartyId.ToString());
                nv.Add("@WarehouseId-INT", obj.WarehouseId.ToString());
                nv.Add("@DeliveryDate-DATETIME", obj.DeliveryDate?.ToString("yyyy-MM-dd"));
                nv.Add("@Remarks-NVARCHAR", obj.Remarks ?? "");
                nv.Add("@Terms_Cond-NVARCHAR", obj.Terms_Cond ?? "");
                nv.Add("@QuoteNo-NVARCHAR", obj.QuoteNo ?? "");
                nv.Add("@QuoteDate-DATETIME", obj.QuoteDate?.ToString("yyyy-MM-dd"));
                nv.Add("@TotalAmount-DECIMAL", obj.TotalAmount.ToString());
                nv.Add("@Cancel-BIT", Convert.ToBoolean(obj.Cancel) ? "1" : "0");
                nv.Add("@Clear-BIT", Convert.ToBoolean(obj.Clear) ? "1" : "0");
                nv.Add("@BranchId-INT", obj.BranchId.ToString());
                nv.Add("@InstanceId-INT", obj.InstanceId.ToString());
                nv.Add("@CreatedBy-INT", obj.CreatedBy.ToString());
                nv.Add("@UpdatedBy-INT", obj.UpdatedBy.ToString());

                dt = _DAL.GetData("sp_purchase_order", nv, _DAL.CSManagementPortalDatabase);

                if (dt == null || dt.Rows.Count == 0)
                    return BadRequest("Save failed");

                int poId = Convert.ToInt32(dt.Rows[0]["POId"]);

                //// ---------------- DELETE OLD DETAILS (ONLY EDIT) ----------------
                //if (obj.Id > 0)
                //{
                //    NameValueCollection del = new NameValueCollection();
                //    del.Add("@POId-INT", poId.ToString());
                //    _DAL.Execute("DELETE FROM Purchase_Order_Detail WHERE POId=@POId", del, _DAL.CSManagementPortalDatabase);
                //}

                // ---------------- INSERT DETAILS ----------------
                foreach (var d in obj.DetailSection)
                {
                    NameValueCollection nd = new NameValueCollection();
                    nd.Add("@POId-INT", poId.ToString());
                    nd.Add("@ProductId-INT", d.ProductId.ToString());
                    nd.Add("@OQty-INT", d.OQty.ToString());
                    nd.Add("@Rate-FLOAT", d.Rate.ToString());
                    nd.Add("@JobNo-NVARCHAR", d.JobNo ?? "");

                    _DAL.InsertData("sp_purchase_order_detail", nd, _DAL.CSManagementPortalDatabase);
                }

                return Ok(new
                {
                    success = true,
                    poId = poId,
                    mode = obj.Id > 0 ? "update" : "insert"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("SavePurchaseOrder: " + ex.Message);
                return BadRequest("Something went wrong");
            }
        }

        #endregion

        #region GetPurchaseOrder
        [HttpGet]
        public IActionResult GetPurchaseOrder()
        {
            DataSet ds = _DAL.GetDataSet("GetPurchaseOrder", null, _DAL.CSManagementPortalDatabase);

            return Ok(new
            {
                Master = ds.Tables[0],
                Detail = ds.Tables.Count > 1 ? ds.Tables[1] : null
            });
        }

        #endregion

        #region GetPurchaseOrderById
        [HttpGet]
        public IActionResult GetPurchaseOrderById(int POId)
        {
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Add("@POId-INT", POId.ToString());
                // Call the stored procedure to get master and detail for this PO
                DataSet ds = _DAL.GetDataSet("GetPurchaseOrderById", nv, _DAL.CSManagementPortalDatabase);

                return Ok(new
                {
                    Master = ds.Tables[0],       // Purchase_Order table (single row)
                    Detail = ds.Tables.Count > 1 ? ds.Tables[1] : null // Purchase_Order_Detail table
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong while fetching the purchase order: " + ex.Message);
            }
        }
        #endregion

        #region DeletePurchaseOrder
        [HttpPost]
        public IActionResult DeletePurchaseOrder([FromBody] Purchase_Order obj)
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Add("@Id-INT", obj.Id.ToString());

            DataTable dt = _DAL.GetData("DeletePurchaseOrder", nv, _DAL.CSManagementPortalDatabase);

            bool success = dt != null &&
                           dt.Rows.Count > 0 &&
                           Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

            return Ok(success);
        }

        #endregion

        #endregion

        #region GRN



        #endregion

        #region Receivable / Sale Module


        #endregion

        #endregion

        // Revenue & Billing Suite

        #region ERP V2 -> Revenue&Billing_Suite

        // All Forms Save/Update API
        #region SaveClientonBoarding
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveClientonBoarding([FromBody] ClientForm client)
        {
            if (client == null)
            {
                return BadRequest("Client object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                // ✅ Check duplicate first
                NameValueCollection nvCheck = new NameValueCollection();
                nvCheck.Add("client_id-INT", client.client_id.ToString());
                nvCheck.Add("client_name-VARCHAR", string.IsNullOrEmpty(client.client_name) ? "NULL" : client.client_name);

                var dtDup = _DAL.GetData("sp_checkduplicateclientname", nvCheck, _DAL.CSManagementPortalDatabase);

                if (dtDup.Rows.Count > 0 && Convert.ToInt32(dtDup.Rows[0]["DuplicateNameCount"]) > 0)
                {
                    return BadRequest(new { success = false, message = "DuplicateNameCount" });
                }


                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflow-VARCHAR", HttpUtility.HtmlEncode(client.workflow));
                nv.Add("instanceid-INT", client.instanceid == null ? "0" : client.instanceid.ToString());
                nv.Add("client_id-INT", client.client_id == null ? "0" : client.client_id.ToString());

                // Client
                nv.Add("client_name-NVARCHAR", client.client_name ?? "");
                nv.Add("industry_id-INT", client.industry_id?.ToString() ?? "0");
                nv.Add("company_size_id-INT", client.company_size_id?.ToString() ?? "0");
                nv.Add("country_id-INT", client.country_id?.ToString() ?? "0");

                nv.Add("website-NVARCHAR", client.website ?? "");
                nv.Add("billing_address-NVARCHAR", client.billing_address ?? "");
                nv.Add("tax_registration_no-NVARCHAR", client.tax_registration_no ?? "");

                // Contact
                nv.Add("primary_contact_name-NVARCHAR", client.primary_contact_name ?? "");
                nv.Add("primary_contact_email-NVARCHAR", client.primary_contact_email ?? "");
                nv.Add("primary_contact_phone-NVARCHAR", client.primary_contact_phone ?? "");

                // Commercial
                nv.Add("contract_type_id-INT",
                    string.IsNullOrEmpty(client.contract_type_id.ToString()) ? "0" : client.contract_type_id.ToString());

                nv.Add("account_owner_id-INT",
                    client.account_owner_id.HasValue ? client.account_owner_id.ToString() : "0");

                nv.Add("support_owner_id-INT",
                    client.support_owner_id.HasValue ? client.support_owner_id.ToString() : "0");


                // Dates (IMPORTANT FIX)
                nv.Add("onboarding_start_date-DATETIME",
                    client.onboarding_start_date.HasValue
                    ? client.onboarding_start_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                nv.Add("onboarding_completion_date-DATETIME",
                    client.onboarding_completion_date.HasValue
                    ? client.onboarding_completion_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                nv.Add("billing_start_date-DATETIME",
                    client.billing_start_date.HasValue
                    ? client.billing_start_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                nv.Add("contract_start_date-DATETIME",
                    client.contract_start_date.HasValue
                    ? client.contract_start_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                nv.Add("contract_end_date-DATETIME",
                    client.contract_end_date.HasValue
                    ? client.contract_end_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                // File Path
                nv.Add("contract_upload-NVARCHAR", client.contract_upload ?? "");
                nv.Add("nda_upload-NVARCHAR", client.nda_upload ?? "");
                nv.Add("proposal_upload-NVARCHAR", client.proposal_upload ?? "");

                // Flags
                nv.Add("high_value_client-BIT", client.high_value_client ? "1" : "0");
                nv.Add("priority_level-VARCHAR", client.priority_level ?? "Low");

                nv.Add("createdby-INT", client.userid.ToString());

                dt = _DAL.GetData("sp_insert_clientform", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_clientform");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_clientform");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region SaveContractForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveContractForm([FromBody] ContractForm client)
        {
            if (client == null)
            {
                return BadRequest("Contract object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflow-VARCHAR", HttpUtility.HtmlEncode(client.workflow));
                nv.Add("instanceid-INT", client.instanceid?.ToString() ?? "0");
                nv.Add("contract_id-INT", client.contract_id?.ToString() ?? "0");

                nv.Add("client_id-INT", client.client_id?.ToString() ?? "0");
                nv.Add("basic_contract_type-INT", client.basic_contract_type?.ToString() ?? "0");
                nv.Add("contract_value-DECIMAL", client.contract_value?.ToString() ?? "0");
                nv.Add("currency_id-INT", client.currency_id?.ToString() ?? "0");
                nv.Add("discount_percent-DECIMAL", client.discount_percent?.ToString() ?? "0");
                nv.Add("tax_percent-DECIMAL", client.tax_percent?.ToString() ?? "0");
                nv.Add("bill_freq_id-INT", client.bill_freq_id?.ToString() ?? "0");

                // Dates (IMPORTANT FIX)
                nv.Add("bill_start_date-DATETIME",
                    client.bill_start_date.HasValue
                    ? client.bill_start_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                // Dates (IMPORTANT FIX)
                nv.Add("bill_end_date-DATETIME",
                    client.bill_end_date.HasValue
                    ? client.bill_end_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");


                nv.Add("billing_cycle-INT", client.billing_cycle?.ToString() ?? "0");

                int month = 0;
                if (!string.IsNullOrEmpty(client.renewal_terms_months))
                {
                    month = Convert.ToInt32(client.renewal_terms_months.Split('-')[1]);
                }

                nv.Add("renewal_terms_months-INT", month.ToString());
                nv.Add("auto_renew-BIT", client.auto_renew ? "1" : "0");
                nv.Add("project_name-NVARCHAR", HttpUtility.HtmlEncode(client.project_name));
                nv.Add("project_value-DECIMAL", client.project_value?.ToString() ?? "0");
                nv.Add("no_of_milestones-INT", client.no_of_milestones?.ToString() ?? "0");
                nv.Add("payment_term_id-INT", client.payment_term_id?.ToString() ?? "0");
                nv.Add("penalty_terms-NVARCHAR", HttpUtility.HtmlEncode(client.penalty_terms));
                nv.Add("support_hours-NVARCHAR", HttpUtility.HtmlEncode(client.support_hours));
                nv.Add("createdby-INT", client.userid?.ToString() ?? "0");

                dt = _DAL.GetData("sp_insert_contractform", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_contractform");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_contractform");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region SaveBillingForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveBillingForm([FromBody] BillingForm client)
        {
            if (client == null)
            {
                return BadRequest("Billing object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflow-VARCHAR", HttpUtility.HtmlEncode(client.workflow));
                nv.Add("instanceid-INT", client.instanceid?.ToString() ?? "0");
                nv.Add("billing_id-INT", client.billing_id?.ToString() ?? "0");

                nv.Add("client_id-INT", client.client_id?.ToString() ?? "0");
                nv.Add("contract_id-INT", client.contract_id?.ToString() ?? "0");
                nv.Add("billing_type_id-INT", client.billing_type_id?.ToString() ?? "0");
                nv.Add("billing_freq_id-INT", client.billing_freq_id?.ToString() ?? "0");

                // Dates (IMPORTANT FIX)
                nv.Add("next_billing_date-DATETIME",
                    client.next_billing_date.HasValue
                    ? client.next_billing_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");


                nv.Add("billing_method-NVARCHAR", HttpUtility.HtmlEncode(client.billing_method));
                nv.Add("delivery_method-NVARCHAR", HttpUtility.HtmlEncode(client.delivery_method));

                nv.Add("createdby-INT", client.userid?.ToString() ?? "0");

                dt = _DAL.GetData("sp_insert_billingform", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_billingform");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_billingform");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region SaveRecurringBillingForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveRecurringBillingForm([FromBody] RecurringBillingForm client)
        {
            if (client == null)
            {
                return BadRequest("Recurring Billing object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflow-VARCHAR", HttpUtility.HtmlEncode(client.workflow));
                nv.Add("instanceid-INT", client.instanceid?.ToString() ?? "0");
                nv.Add("rbilling_id-INT", client.rbilling_id?.ToString() ?? "0");

                nv.Add("client_id-INT", client.client_id?.ToString() ?? "0");
                nv.Add("contract_id-INT", client.contract_id?.ToString() ?? "0");

                // Billing Period
                nv.Add("bill_start_date-DATETIME",
                    client.bill_start_date.HasValue
                    ? client.bill_start_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                nv.Add("bill_end_date-DATETIME",
                    client.bill_end_date.HasValue
                    ? client.bill_end_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                // Financial Summary
                nv.Add("amount-DECIMAL", client.amount?.ToString() ?? "0");
                nv.Add("tax-DECIMAL", client.tax?.ToString() ?? "0");
                nv.Add("total_amount-DECIMAL", client.total_amount?.ToString() ?? "0");

                nv.Add("due_date-DATETIME",
                    client.due_date.HasValue
                    ? client.due_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                nv.Add("bill_status_id-INT", client.bill_status_id?.ToString() ?? "0");

                nv.Add("createdby-INT", client.userid?.ToString() ?? "0");

                dt = _DAL.GetData("sp_insert_recurringbillingform", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_recurringbillingform");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_recurringbillingform");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region SaveMilestone
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveMilestone([FromBody] MilestoneForm milestone)
        {
            if (milestone == null)
            {
                return BadRequest("Milestone object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflow-VARCHAR", HttpUtility.HtmlEncode(milestone.workflow));
                nv.Add("instanceid-INT", milestone.instanceid?.ToString() ?? "0");
                nv.Add("milestone_id-INT", milestone.milestone_id?.ToString() ?? "0");

                nv.Add("client_id-INT", milestone.client_id?.ToString() ?? "0");
                nv.Add("contract_id-INT", milestone.contract_id?.ToString() ?? "0");
                nv.Add("milestone_name-NVARCHAR", milestone.milestone_name ?? "");

                nv.Add("milestone_amount-DECIMAL", milestone.milestone_amount?.ToString() ?? "0");

                nv.Add("exp_completion_date-DATETIME",
                    milestone.exp_completion_date.HasValue
                    ? milestone.exp_completion_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                nv.Add("completion_date-DATETIME",
                    milestone.completion_date.HasValue
                    ? milestone.completion_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                nv.Add("milestone_desc-NVARCHAR", milestone.milestone_desc ?? "");
                nv.Add("internal_owner_id-INT", milestone.internal_owner_id?.ToString() ?? "0");
                nv.Add("client_approver_id-INT", milestone.client_approver_id?.ToString() ?? "0");

                nv.Add("deliverables_list-NVARCHAR", milestone.deliverables_list ?? "");
                nv.Add("dependencies-NVARCHAR", milestone.dependencies ?? "");

                // =========================
                // DOCUMENT
                // =========================
                nv.Add("docUpload-NVARCHAR", milestone.docUpload ?? "");

                nv.Add("client_approval-NVARCHAR", milestone.client_approval ?? "");
                nv.Add("client_comments-NVARCHAR", milestone.client_comments ?? "");
                nv.Add("qa_comments-NVARCHAR", milestone.qa_comments ?? "");
                nv.Add("createdby-INT", milestone.userid?.ToString() ?? "0");

                dt = _DAL.GetData("sp_insert_milestone", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_milestone");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_milestone");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region SaveInvoiceForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveInvoiceForm([FromBody] InvoiceForm payload)
        {
            if (payload == null)
            {
                return BadRequest("Invoice object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflow-VARCHAR", payload.workflow ?? "");
                nv.Add("instanceid-INT", payload.instanceid?.ToString() ?? "0");

                nv.Add("invoice_id-INT", payload.invoice_id?.ToString() ?? "0");
                nv.Add("invoice_no-NVARCHAR", payload.invoice_no ?? "");
                nv.Add("invoice_date-DATETIME",
                    payload.invoice_date?.ToString("yyyy-MM-dd") ?? "NULL");
                nv.Add("due_date-DATETIME",
                    payload.due_date?.ToString("yyyy-MM-dd") ?? "NULL");

                nv.Add("client_Id-INT", payload.client_Id?.ToString() ?? "0");
                nv.Add("contract_Id-INT", payload.contract_Id?.ToString() ?? "0");

                nv.Add("billing_type_id-INT", payload.billing_type_id?.ToString() ?? "0");
                nv.Add("rbilling_Id-INT", payload.rbilling_Id?.ToString() ?? "0");
                nv.Add("milestone_Id-INT", payload.milestone_Id?.ToString() ?? "0");

                nv.Add("subtotal-DECIMAL", payload.subtotal?.ToString() ?? "0");
                nv.Add("taxtotal-DECIMAL", payload.taxtotal?.ToString() ?? "0");
                nv.Add("grandtotal-DECIMAL", payload.grandtotal?.ToString() ?? "0");

                // File Path
                nv.Add("attach1_upload-NVARCHAR", payload.attach1_upload ?? "");
                nv.Add("attach2_upload-NVARCHAR", payload.attach2_upload ?? "");

                nv.Add("createdby-INT", payload.userid.ToString());

                // ================= MASTER INSERT =================
                dt = _DAL.GetData("sp_invoice_form_save", nv, _DAL.CSManagementPortalDatabase);

                int invoiceId = Convert.ToInt32(dt.Rows[0]["invoice_id"]);


                // ================= DETAIL DELETE FOR EDIT =================
                if (payload.invoice_id > 0)
                {
                    NameValueCollection ndel = new NameValueCollection();

                    ndel.Add("invoice_Id-INT", invoiceId.ToString());
                    _DAL.GetData("sp_invoice_detail_delete", ndel, _DAL.CSManagementPortalDatabase);
                }


                // ================= DETAIL INSERT =================
                if (payload.invoice_details != null && payload.invoice_details.Count > 0)
                {
                    foreach (var item in payload.invoice_details)
                    {
                        NameValueCollection nd = new NameValueCollection();

                        nd.Add("invoice_Id-INT", invoiceId.ToString());
                        nd.Add("item_desc-NVARCHAR", item.item_desc ?? "");
                        nd.Add("quantity-DECIMAL", item.quantity?.ToString() ?? "0");
                        nd.Add("unit_price-DECIMAL", item.unit_price?.ToString() ?? "0");
                        nd.Add("amount-DECIMAL", item.amount?.ToString() ?? "0");
                        nd.Add("tax_percent-DECIMAL", item.tax_percent?.ToString() ?? "0");
                        nd.Add("tax_amount-DECIMAL", item.tax_amount?.ToString() ?? "0");
                        nd.Add("total_amount-DECIMAL", item.total_amount?.ToString() ?? "0");

                        _DAL.GetData("sp_invoice_detail_save", nd, _DAL.CSManagementPortalDatabase);
                    }
                }

                nv = null;
                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_invoice_form_save");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_invoice_form_save");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region SavePaymentForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SavePaymentForm([FromBody] PaymentForm payload)
        {
            if (payload == null)
            {
                return BadRequest("Payment object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflow-VARCHAR", HttpUtility.HtmlEncode(payload.workflow));
                nv.Add("instanceid-INT", payload.instanceid == null ? "0" : payload.instanceid.ToString());
                nv.Add("payment_id-INT", payload.payment_id == null ? "0" : payload.payment_id.ToString());

                nv.Add("client_id-INT", payload.client_id == null ? "0" : payload.client_id.ToString());
                nv.Add("invoice_id-INT", payload.invoice_id == null ? "0" : payload.invoice_id.ToString());

                nv.Add("amount_received-DECIMAL", payload.amount_received?.ToString() ?? "0");
                nv.Add("payment_mode-NVARCHAR", payload.payment_mode ?? "");
                nv.Add("chq_no-NVARCHAR", payload.chq_no ?? "");
                nv.Add("bank_name-NVARCHAR", payload.bank_name ?? "");

                // Dates (IMPORTANT FIX)
                nv.Add("value_date-DATETIME",
                    payload.value_date.HasValue
                    ? payload.value_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                // File Path
                nv.Add("receipt_upload-NVARCHAR", payload.receipt_upload ?? "");

                nv.Add("createdby-INT", payload.userid.ToString());

                dt = _DAL.GetData("sp_insert_paymentform_info", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_paymentform_info");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_paymentform_info");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region SaveARExceptionForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveARExceptionForm([FromBody] ARExceptionForm payload)
        {
            if (payload == null)
            {
                return BadRequest("AR Exception object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflow-VARCHAR", HttpUtility.HtmlEncode(payload.workflow));
                nv.Add("instanceid-INT", payload.instanceid?.ToString() ?? "0");
                nv.Add("exception_id-INT", payload.exception_id?.ToString() ?? "0");

                nv.Add("client_id-INT", payload.client_id?.ToString() ?? "0");
                nv.Add("invoice_id-INT", payload.invoice_id?.ToString() ?? "0");

                nv.Add("amount-DECIMAL", payload.amount?.ToString() ?? "0");
                nv.Add("days_overdue-INT", payload.days_overdue?.ToString() ?? "0");

                nv.Add("risk_category-NVARCHAR", HttpUtility.HtmlEncode(payload.risk_category));
                nv.Add("escalation_level-NVARCHAR", HttpUtility.HtmlEncode(payload.escalation_level));
                nv.Add("notes-NVARCHAR", HttpUtility.HtmlEncode(payload.notes));

                nv.Add("createdby-INT", payload.userid?.ToString() ?? "0");

                dt = _DAL.GetData("sp_insert_arexceptionform", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_arexceptionform");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_arexceptionform");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region SaveRenewalForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveRenewalForm([FromBody] RenewalForm payload)
        {
            if (payload == null)
            {
                return BadRequest("Renewal object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflow-VARCHAR", HttpUtility.HtmlEncode(payload.workflow));
                nv.Add("instanceid-INT", payload.instanceid == null ? "0" : payload.instanceid.ToString());
                nv.Add("renew_id-INT", payload.renew_id == null ? "0" : payload.renew_id.ToString());

                nv.Add("client_id-INT", payload.client_id == null ? "0" : payload.client_id.ToString());
                nv.Add("contract_id-INT", payload.contract_id == null ? "0" : payload.contract_id.ToString());

                nv.Add("renewal_term-NVARCHAR", payload.renewal_term ?? "");
                nv.Add("contract_value-DECIMAL", payload.contract_value?.ToString() ?? "0");
                nv.Add("discount-INT", payload.discount?.ToString() ?? "0");

                // Dates (IMPORTANT FIX)
                nv.Add("start_date-DATETIME",
                    payload.start_date.HasValue
                    ? payload.start_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");

                nv.Add("end_date-DATETIME",
                    payload.end_date.HasValue
                    ? payload.end_date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "NULL");


                // File Path
                nv.Add("doc_upload-NVARCHAR", payload.doc_upload ?? "");

                nv.Add("createdby-INT", payload.userid.ToString());

                dt = _DAL.GetData("sp_insert_renewalform", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_renewalform");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_renewalform");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion




        // All Forms ViewGrid API
        #region ViewClientForm
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetViewAllClientForm()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getallclientinfo", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getallclientinfo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region ViewContractForm
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetViewAllContractForm()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getallcontractinfo", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getallcontractinfo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region ViewBillingForm
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetViewAllBillingForm()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getallbillinginfo", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getallbillinginfo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region ViewBillingForm
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetViewAllRecurringBillingForm()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getallrecurringbillinginfo", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getallrecurringbillinginfo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region ViewMilestoneForm
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetViewAllMilestoneForm()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getallmilestoneinfo", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getallmilestoneinfo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region ViewInvoiceForm
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetViewAllInvoiceForm()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getallinvoiceinfo", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getallinvoiceinfo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region ViewPaymentForm
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetViewAllPaymentForm()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getallpaymentform_info", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getallpaymentform_info");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion

        #region ViewContractForm
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetViewAllARExceptionForm()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getallarexceptioninfo", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getallarexceptioninfo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion      

        #region ViewRenewalForm
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetViewAllRenewalForm()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                //nv.Add("CategoryCode-INT", CategoryCode);
                dt = _DAL.GetData("sp_getallrenewalform_info", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getallrenewalform_info");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "TransController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        #endregion



        // Fill Form in Edit Mode .....
        #region FillClientonBoarding
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetClientById(int clientId)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("clientId-INT", clientId.ToString());

                dt = _DAL.GetData("sp_select_ClientDetailByClientId", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_select_ClientDetailByClientId");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_select_ClientDetailByClientId");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt != null)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region FillContractForm
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetContractById(int contractId)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("contractId-INT", contractId.ToString());

                dt = _DAL.GetData("sp_select_contractdetailById", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_select_contractdetailById");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_select_contractdetailById");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt != null)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region FillBillingForm
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetBillingById(int billingId)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("billingId-INT", billingId.ToString());

                dt = _DAL.GetData("sp_select_billingdetailById", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_select_billingdetailById");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_select_billingdetailById");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt != null)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region FillBillingForm
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetRecurringBillingById(int rbillingId)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("rbillingId-INT", rbillingId.ToString());

                dt = _DAL.GetData("sp_select_recurringbillingdetailById", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_select_recurringbillingdetailById");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_select_recurringbillingdetailById");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt != null)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region FillMilestone
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetMilestoneById(int milestoneId)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("milestoneId-INT", milestoneId.ToString());

                dt = _DAL.GetData("sp_select_milestoneById", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_select_milestoneById");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_select_milestoneById");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt != null)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region FillInvoiceForm-Master
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetInvoiceById(int invoiceId)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("invoiceId-INT", invoiceId.ToString());

                dt = _DAL.GetData("sp_select_invoiceById", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_select_invoiceById");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_select_invoiceById");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt != null)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region FillInvoiceForm-Detail
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetInvoiceDetailByInvoiceId(int invoiceId)
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Add("invoiceId-INT", invoiceId.ToString());

            DataTable dt = _DAL.GetData("sp_select_invoiceDetailByInvoiceId", nv, _DAL.CSManagementPortalDatabase);
            return Ok(dt);
        }
        #endregion

        #region FillPaymentForm
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetPaymentFormById(int paymentId)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("paymentId-INT", paymentId.ToString());

                dt = _DAL.GetData("sp_select_paymentform_infoById", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_select_paymentform_infoById");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_select_paymentform_infoById");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt != null)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region FillARExceptionForm
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetARExceptionById(int exceptionId)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("exceptionId-INT", exceptionId.ToString());

                dt = _DAL.GetData("sp_select_arexceptiondetailById", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_select_arexceptiondetailById");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_select_arexceptiondetailById");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt != null)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion

        #region FillRenewalForm
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetRenewalFormById(int renewId)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("renewId-INT", renewId.ToString());

                dt = _DAL.GetData("sp_select_renewalform_infoById", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_select_renewalform_infoById");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_select_renewalform_infoById");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt != null)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        #endregion




        // Delete Api .....
        #region DeleteClientForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteClientForm([FromBody] DeleteData obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PrimaryID == null ? "0" : obj.PrimaryID.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deleteclientinfo", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deleteclientinfo");
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

        #region DeleteContractForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteContractForm([FromBody] DeleteData obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PrimaryID == null ? "0" : obj.PrimaryID.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deletecontractinfo", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deletecontractinfo");
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

        #region DeleteBillingForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteBillingForm([FromBody] DeleteData obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PrimaryID == null ? "0" : obj.PrimaryID.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deletebillinginfo", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deletebillinginfo");
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

        #region DeleteRecurringBillingForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteRecurringBillingForm([FromBody] DeleteData obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PrimaryID == null ? "0" : obj.PrimaryID.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deleterecurringbillinginfo", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deleterecurringbillinginfo");
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

        #region DeleteMilestoneForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteMilestoneForm([FromBody] DeleteData obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PrimaryID == null ? "0" : obj.PrimaryID.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deletemilestoneinfo", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deletemilestoneinfo");
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

        #region DeleteInvoiceForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteInvoiceForm([FromBody] DeleteData obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PrimaryID == null ? "0" : obj.PrimaryID.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deleteinvoiceinfo", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deleteinvoiceinfo");
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

        #region DeletePaymentForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeletePaymentForm([FromBody] DeleteData obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PrimaryID == null ? "0" : obj.PrimaryID.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deletepaymentform_info", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deletepaymentform_info");
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

        #region DeleteARExceptionForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteARExceptionForm([FromBody] DeleteData obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PrimaryID == null ? "0" : obj.PrimaryID.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deletearexceptioninfo", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deletearexceptioninfo");
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

        #region DeleteRenewalForm
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteRenewalForm([FromBody] DeleteData obj)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("Id-INT", obj.PrimaryID == null ? "0" : obj.PrimaryID.ToString());

                // Use GetData to get the RowsDeleted
                DataTable dt = _DAL.GetData("sp_deleterenewalform_info", nv, _DAL.CSManagementPortalDatabase);

                Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_deleterenewalform_info");
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


        #endregion


    }
}
