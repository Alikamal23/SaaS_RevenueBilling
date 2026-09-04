using RevenueBillingApi.Context;
using RevenueBillingApi.Extensions;
using RevenueBillingApi.Models.Authentication;
using RevenueBillingApi.Models.DMS;
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
using System.Security.Claims;
using System.Text;
using System.Web;

namespace RevenueBillingApi.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/{controller}/{action}/{id:int?}")]
    [ApiController]
    public class DMSController : ControllerBase
    {
        private readonly DataAccessLayer _DAL;
        private readonly SendEmail _sendemail;
        private readonly ILogger<DMSController> _logger;
        private readonly DataEncryptor _dataencryptor;
        private readonly RandomStringGenerator _randomstringgenerator;
        private readonly CommonMethods _CommonMethods;
        public DMSController(DataAccessLayer DAL, ILogger<DMSController> logger, SendEmail sendemail, DataEncryptor dataencryptor, RandomStringGenerator randomstringgenerator, CommonMethods commonMethods)
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

        //Ali Kamal Work on Molti DMS 24/Oct/2025
        //#region DMS

        //#region KeyFactors
        //[RateLimitMiddleware(100, 5)]
        //[HttpGet]
        //public IActionResult GetKeyFactors()
        //{
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        dt = _DAL.GetData("sp_getkeyfactors", null, _DAL.CSManagementPortalDatabase);

        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getkeyfactors");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
        //        SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
        //        BadRequest(ex.Message);
        //    }
        //    return Ok(dt);
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult SaveKeyFactors([FromBody] KeyFactors obj)
        //{
        //    DataTable dt = null;
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Add("FactorId-INT", obj.FactorId.ToString());
        //        nv.Add("FactorName-VARCHAR", string.IsNullOrEmpty(obj.FactorName) ? "NULL" : obj.FactorName);
        //        nv.Add("IsActive-BIT", obj.IsActive.HasValue && obj.IsActive.Value ? "1" : "0");

        //        dt = _DAL.GetData("sp_savefactor", nv, _DAL.CSManagementPortalDatabase);

        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_savefactor");
        //            return Ok(dt);
        //        }
        //        else
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_savefactor");
        //            return BadRequest("No rows affected");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
        //        SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
        //        return BadRequest("Something Went Wrong Please Contact Your System Administrator");
        //    }
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult EditKeyFactors([FromBody] KeyFactors obj)
        //{
        //    bool Result = false;
        //    try
        //    {
        //        NameValueCollection? nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("FactorId-INT", obj.FactorId == null ? "0" : obj.FactorId.ToString());
        //        nv.Add("FactorName-VARCHAR", obj.FactorName == null ? "NULL" : obj.FactorName);
        //        nv.Add("IsActive-BIT", obj.IsActive == null ? "false" : obj.IsActive.ToString());
        //        Result = _DAL.InsertData("sp_savefactor", nv, _DAL.CSManagementPortalDatabase);
        //        nv = null;

        //        if (Result)
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_savefactor");
        //        }
        //        else
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_savefactor");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
        //        SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
        //        return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
        //    }

        //    if (Result)
        //    {
        //        return Ok(Result);
        //    }
        //    else
        //    {
        //        return BadRequest(Result);
        //    }
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult DeleteKeyFactors([FromBody] KeyFactors obj)
        //{
        //    bool Result = false;

        //    try
        //    {
        //        NameValueCollection? nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("FactorId-INT", obj.FactorId == null ? "0" : obj.FactorId.ToString());

        //        // Use GetData to get the RowsDeleted
        //        DataTable dt = _DAL.GetData("sp_deletefactor", nv, _DAL.CSManagementPortalDatabase);

        //        Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

        //        nv = null;

        //        if (Result)
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_delete_user");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
        //        SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
        //        return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
        //    }

        //    return Ok(Result);
        //}
        //#endregion

        //#region KPISurveyForm
        //[RateLimitMiddleware(100, 5)]
        //[HttpGet]
        //public IActionResult GetAllKeyFactors()
        //{
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        NameValueCollection? nv = new NameValueCollection();
        //        nv.Clear();
        //        dt = _DAL.GetData("sp_select_keyfactors", nv, _DAL.CSManagementPortalDatabase);

        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_keyfactors");
        //        }
        //        else
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_keyfactors");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
        //        SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
        //        return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
        //    }

        //    return Ok(dt);

        //}

        //[RateLimitMiddleware(100, 5)]
        //[HttpGet]
        //public IActionResult GetKPISurveyForm()
        //{
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        dt = _DAL.GetData("sp_getquestions", null, _DAL.CSManagementPortalDatabase);

        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_getkeyfactors");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
        //        SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
        //        BadRequest(ex.Message);
        //    }
        //    return Ok(dt);
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult SaveKPISurveyForm([FromBody] KPISurveyForm obj)
        //{
        //    DataTable dt = null;
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Add("QuestionId-INT", obj.QuestionId == null ? "0" : obj.QuestionId.ToString());
        //        nv.Add("Question-VARCHAR", obj.Question == null ? "NULL" : obj.Question);
        //        nv.Add("FactorId-INT", obj.FactorId == null ? "0" : obj.FactorId.ToString());
        //        nv.Add("IsActive-BIT", obj.IsActive == null ? "false" : obj.IsActive.ToString());
        //        nv.Add("UserID-INT", obj.UserID == null ? "0" : obj.UserID.ToString());
        //        dt = _DAL.GetData("sp_savequestion", nv, _DAL.CSManagementPortalDatabase);

        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_savefactor");
        //            return Ok(dt);
        //        }
        //        else
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_savefactor");
        //            return BadRequest("No rows affected");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
        //        SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
        //        return BadRequest("Something Went Wrong Please Contact Your System Administrator");
        //    }
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult EditKPISurveyForm([FromBody] KPISurveyForm obj)
        //{
        //    bool Result = false;
        //    try
        //    {
        //        NameValueCollection? nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("QuestionId-INT", obj.QuestionId == null ? "0" : obj.QuestionId.ToString());
        //        nv.Add("Question-VARCHAR", obj.Question == null ? "NULL" : obj.Question);
        //        nv.Add("FactorId-INT", obj.FactorId == null ? "0" : obj.FactorId.ToString());
        //        nv.Add("IsActive-BIT", obj.IsActive == null ? "false" : obj.IsActive.ToString());
        //        nv.Add("UserID-INT", obj.UserID == null ? "0" : obj.UserID.ToString());
        //        Result = _DAL.InsertData("sp_savequestion", nv, _DAL.CSManagementPortalDatabase);
        //        nv = null;

        //        if (Result)
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_savefactor");
        //        }
        //        else
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_savefactor");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
        //        SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
        //        return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
        //    }

        //    if (Result)
        //    {
        //        return Ok(Result);
        //    }
        //    else
        //    {
        //        return BadRequest(Result);
        //    }
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult DeleteKPISurveyForm([FromBody] KPISurveyForm obj)
        //{
        //    bool Result = false;

        //    try
        //    {
        //        NameValueCollection? nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("QuestionId-INT", obj.QuestionId == null ? "0" : obj.QuestionId.ToString());

        //        // Use GetData to get the RowsDeleted
        //        DataTable dt = _DAL.GetData("sp_deletequestion", nv, _DAL.CSManagementPortalDatabase);

        //        Result = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["RowsDeleted"]) > 0;

        //        nv = null;

        //        if (Result)
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_delete_user");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
        //        SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
        //        return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
        //    }

        //    return Ok(Result);
        //}

        //#endregion

        //#region DealerUploader

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult LoadZone_DDL()
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        dataTable = _DAL.GetData("All_zonesforExcel", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult DownloadExcel([FromBody] DownloadParam model)
        //{
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("Monthd-INT", model.Monthd.ToString());
        //        nv.Add("Year-INT", model.Year.ToString());
        //        nv.Add("Zone-INT", model.Zone.ToString());
        //        nv.Add("Type-VARCHAR", model.type);

        //        DataTable dt = _DAL.GetData("MRDoctorExcelFile3", nv, _DAL.CSManagementPortalDatabase);
        //        nv = null;

        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            return Ok(new
        //            {
        //                statusCode = 200,
        //                responseMsg = "Request Successfully!",
        //                data = dt
        //            });
        //        }
        //        else
        //        {
        //            return Ok(new
        //            {
        //                statusCode = 204,
        //                responseMsg = "No Data Found!",
        //                data = new object[] { }
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new
        //        {
        //            statusCode = 500,
        //            responseMsg = "Something went wrong!",
        //            data = ex.Message
        //        });
        //    }
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public async Task<IActionResult> UploadExcel([FromForm] UploadParam model, IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest(new { statusCode = 400, responseMsg = "No file uploaded." });

        //    try
        //    {
        //        //ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        //        //var dealerList = new List<UploadDoctorModel>();

        //        //using (var stream = new MemoryStream())
        //        //{
        //        //    await file.CopyToAsync(stream);
        //        //    using (var package = new ExcelPackage(stream))
        //        //    {
        //        //        var worksheet = package.Workbook.Worksheets[0];
        //        //        int rowCount = worksheet.Dimension.Rows;

        //        //        // Start from row 3 → Skip NOTE & Header
        //        //        for (int row = 3; row <= rowCount; row++)
        //        //        {
        //        //            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
        //        //                continue;

        //        //            var dealer = new UploadDoctorModel
        //        //            {
        //        //                SPOID = Convert.ToInt32(worksheet.Cells[row, 1].Text),
        //        //                TerritoryID = worksheet.Cells[row, 2].Text,
        //        //                DealerCode = worksheet.Cells[row, 3].Text,
        //        //                DealerName = worksheet.Cells[row, 4].Text,
        //        //                ContactPersonName = worksheet.Cells[row, 5].Text,
        //        //                MainGroup = worksheet.Cells[row, 6].Text,
        //        //                Type = worksheet.Cells[row, 7].Text,
        //        //                Address = worksheet.Cells[row, 8].Text,
        //        //                City = worksheet.Cells[row, 9].Text,
        //        //                MobileNumber = worksheet.Cells[row, 10].Text,
        //        //                Frequency = worksheet.Cells[row, 11].Text,
        //        //                KOL = worksheet.Cells[row, 12].Text,
        //        //                STATUS = worksheet.Cells[row, 13].Text,
        //        //                Potential = worksheet.Cells[row, 14].Text,
        //        //                Propensity = worksheet.Cells[row, 15].Text,
        //        //                Flag = worksheet.Cells[row, 16].Text,
        //        //                SPONAME = worksheet.Cells[row, 17].Text,
        //        //                Brand = worksheet.Cells[row, 18].Text,
        //        //                Market_Name = worksheet.Cells[row, 19].Text
        //        //            };

        //        //            dealerList.Add(dealer);
        //        //        }
        //        //    }
        //        //}

        //        //// Insert data into database
        //        //foreach (var dealer in dealerList)
        //        //{
        //        //    NameValueCollection nv = new NameValueCollection();
        //        //    nv.Clear();
        //        //    nv.Add("Monthd-INT", model.Monthd.ToString());
        //        //    nv.Add("Year-INT", model.Year.ToString());
        //        //    nv.Add("Zone-INT", model.Zone.ToString());
        //        //    nv.Add("SPOID-INT", dealer.SPOID.ToString());
        //        //    nv.Add("TerritoryID-VARCHAR", dealer.TerritoryID);
        //        //    nv.Add("DealerCode-VARCHAR", dealer.DealerCode);
        //        //    nv.Add("DealerName-VARCHAR", dealer.DealerName);
        //        //    nv.Add("ContactPersonName-VARCHAR", dealer.ContactPersonName);
        //        //    nv.Add("MainGroup-VARCHAR", dealer.MainGroup);
        //        //    nv.Add("Type-VARCHAR", dealer.Type);
        //        //    nv.Add("Address-VARCHAR", dealer.Address);
        //        //    nv.Add("City-VARCHAR", dealer.City);
        //        //    nv.Add("MobileNumber-VARCHAR", dealer.MobileNumber);
        //        //    nv.Add("Frequency-VARCHAR", dealer.Frequency);
        //        //    nv.Add("KOL-VARCHAR", dealer.KOL);
        //        //    nv.Add("STATUS-VARCHAR", dealer.STATUS);
        //        //    nv.Add("Potential-VARCHAR", dealer.Potential);
        //        //    nv.Add("Propensity-VARCHAR", dealer.Propensity);
        //        //    nv.Add("Flag-VARCHAR", dealer.Flag);
        //        //    nv.Add("SPONAME-VARCHAR", dealer.SPONAME);
        //        //    nv.Add("Brand-VARCHAR", dealer.Brand);
        //        //    nv.Add("Market_Name-VARCHAR", dealer.Market_Name);

        //        //    bool isInserted = _DAL.InsertData("AddXLXDoctor3", nv, _DAL.CSManagementPortalDatabase);
        //        //    if (!isInserted)
        //        //    {
        //        //        return BadRequest(new
        //        //        {
        //        //            statusCode = 500,
        //        //            responseMsg = "Error inserting dealer data.",
        //        //        });
        //        //    }
        //        //}

        //        //return Ok(new
        //        //{
        //        //    statusCode = 200,
        //        //    responseMsg = "File uploaded and dealer data processed successfully.",
        //        //    data = dealerList.Count
        //        //});
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new
        //        {
        //            statusCode = 500,
        //            responseMsg = "Something went wrong!",
        //            data = ex.Message
        //        });
        //    }

        //    return Ok();


        //}

        //#endregion


        ////working ....

        //#region MasterDealer

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetZones()
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();

        //        dataTable = _DAL.GetData("sp_selectzone", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetUserNames(string? zoneid)
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("zoneid-INT", zoneid);
        //        dataTable = _DAL.GetData("sp_select_usernames", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return Ok(dataTable = null);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetDealers(string? dealertype)
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("dealertype-INT", dealertype);
        //        dataTable = _DAL.GetData("sp_select_dealers", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return Ok(dataTable = null);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }




        //}

        //[HttpGet]
        //public IActionResult GetDealerInfo(string? dealerid)
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("dealerid-INT", dealerid);
        //        dataTable = _DAL.GetData("sp_select_dealerinfo", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return Ok(dataTable = null);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult AssignDealer(DealerAssign dealerAssign)
        //{
        //    bool isinserted = false;

        //    NameValueCollection nv = new NameValueCollection();


        //    string[] monthyear = dealerAssign.monthofdealer.Split("-");

        //    string month = monthyear[1];
        //    string year = monthyear[0];

        //    nv.Clear();
        //    nv.Add("dealerid-INT", dealerAssign.dealerid);
        //    nv.Add("userids-VARCHAR", dealerAssign.userids);
        //    nv.Add("month-INT", month);
        //    nv.Add("year-INT", year);
        //    isinserted = _DAL.InsertData("sp_assign_dealer", nv, _DAL.CSManagementPortalDatabase);

        //    return Ok();
        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetTemplate(string monthyear, string zoneid)
        //{
        //    DataTable dt = new DataTable();
        //    NameValueCollection nv = new NameValueCollection();

        //    string[] my = monthyear.Split("-");
        //    string year = my[0];
        //    string month = my[1];


        //    nv.Clear();
        //    nv.Add("month-INT", month);
        //    nv.Add("year-INT", year);
        //    nv.Add("zone-INT", zoneid);
        //    dt = _DAL.GetData("sp_get_dealertemplate", nv, _DAL.CSManagementPortalDatabase);

        //    return Ok(dt);
        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult AssignDealerwithUploader()
        //{
        //    return Ok();
        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult DealerUpload([FromForm] DealerUploader dealerBoarding)
        //{

        //    if (dealerBoarding == null || dealerBoarding.files == null || dealerBoarding.files.Length == 0)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    NameValueCollection nv = new NameValueCollection();

        //    string[] my = dealerBoarding.monthyear?.Split("-");
        //    if (my == null || my.Length != 2)
        //    {
        //        return BadRequest("Invalid monthyear format.");
        //    }

        //    string year = my[0];
        //    string month = my[1];

        //    using (var stream = dealerBoarding.files.OpenReadStream())
        //    {
        //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //        ExcelPackage package = new ExcelPackage();
        //        DataTable table = new DataTable();
        //        package.Load(stream);
        //        if (package.Workbook.Worksheets.Count > 0)
        //        {
        //            using (ExcelWorksheet workSheet = package.Workbook.Worksheets.First())
        //            {
        //                int noOfCol = workSheet.Dimension.End.Column;
        //                int noOfRow = workSheet.Dimension.End.Row;
        //                int rowIndex = 1;

        //                for (int c = 1; c <= noOfCol; c++)
        //                {
        //                    table.Columns.Add(workSheet.Cells[rowIndex, c].Text.Replace(" ", "_").ToLower());
        //                }
        //                rowIndex = 2;
        //                for (int r = rowIndex; r <= noOfRow; r++)
        //                {
        //                    DataRow dr = table.NewRow();
        //                    for (int c = 1; c <= noOfCol; c++)
        //                    {
        //                        dr[c - 1] = workSheet.Cells[r, c].Value;
        //                    }
        //                    table.Rows.Add(dr);
        //                }
        //            }
        //        }

        //        DataTable errordt = table.Copy();
        //        errordt.Columns.Add("errormessage", typeof(string));
        //        errordt.AcceptChanges();

        //        errordt.Rows.Clear();

        //        if (table.Rows.Count > 0)
        //        {
        //            foreach (DataRow row in table.Rows)
        //            {
        //                string errormessage = string.Empty;

        //                string? userid = string.Empty;
        //                string? dealertypeid = string.Empty;
        //                string? categoryid = string.Empty;
        //                string? cityid = string.Empty;
        //                string? zoneid = string.Empty;

        //                bool isinserted = false;
        //                DataTable dt = new DataTable();
        //                string? distributorcnic = row["cnic"].ToString().Trim();
        //                string? distributorcontact = row["dealer_contact_number"].ToString().Trim();
        //                string? usrname = row["username"].ToString().Trim();
        //                string? categoryname = row["category"].ToString().Trim();
        //                string? dealer_type = row["dealer_type"].ToString().Trim();
        //                string? dealership_name = row["dealership_name"].ToString().Trim();
        //                string? owner_name = row["owner_name"].ToString().Trim();
        //                string? brand = row["brand"].ToString().Trim();
        //                string? area = row["area"].ToString().Trim();
        //                string? zone = row["zone"].ToString().Trim();
        //                string? city = row["city"].ToString().Trim();
        //                // string? main_group = row["main_group"].ToString();
        //                string? address = row["address"].ToString().Trim();
        //                string? dealercode = row["dealercode"].ToString().Trim();  //have to insert


        //                nv.Clear();
        //                nv.Add("username-VARCHAR", usrname);
        //                dt = _DAL.GetData("sp_selectuser_onusername", nv, _DAL.CSManagementPortalDatabase);
        //                if (dt.Rows.Count > 0)
        //                {
        //                    userid = dt.Rows[0][0].ToString();
        //                }
        //                else
        //                {
        //                    errormessage += "User does not exist in system";
        //                }


        //                dt.Clear();
        //                nv.Clear();
        //                nv.Add("dealer_type-VARCHAR", dealer_type);
        //                dt = _DAL.GetData("sp_select_dealertypeid", nv, _DAL.CSManagementPortalDatabase);
        //                if (dt.Rows.Count > 0)
        //                {
        //                    dealertypeid = dt.Rows[0][0].ToString();
        //                }
        //                else
        //                {
        //                    errormessage += "Dealer Type does not exist in system";
        //                }


        //                dt.Clear();
        //                nv.Clear();
        //                nv.Add("category-VARCHAR", categoryname);
        //                dt = _DAL.GetData("sp_select_categoryid", nv, _DAL.CSManagementPortalDatabase);
        //                if (dt.Rows.Count > 0)
        //                {
        //                    categoryid = dt.Rows[0][0].ToString();
        //                }
        //                else
        //                {
        //                    errormessage += "Category does not exist in system";
        //                }


        //                dt.Clear();
        //                nv.Clear();
        //                nv.Add("city-VARCHAR", city);
        //                dt = _DAL.GetData("sp_select_cityid", nv, _DAL.CSManagementPortalDatabase);
        //                if (dt.Rows.Count > 0)
        //                {
        //                    cityid = dt.Rows[0][0].ToString();
        //                }
        //                else
        //                {
        //                    errormessage += "City does not exist in system";

        //                }

        //                dt.Clear();
        //                nv.Clear();
        //                nv.Add("zone-VARCHAR", zone);
        //                dt = _DAL.GetData("sp_select_zoneid", nv, _DAL.CSManagementPortalDatabase);
        //                if (dt.Rows.Count > 0)
        //                {
        //                    zoneid = dt.Rows[0][0].ToString();
        //                }
        //                else
        //                {
        //                    errormessage += "Zone does not exist in system";

        //                }


        //                if (userid != string.Empty && categoryid != string.Empty && zoneid != string.Empty && cityid != string.Empty)
        //                {
        //                    if (row["flag"].ToString() == "N")
        //                    {
        //                        try
        //                        {
        //                            nv.Clear();
        //                            nv.Add("userid-INT", userid);
        //                            nv.Add("dealername-VARCHAR", dealership_name);
        //                            nv.Add("dealertypeid-VARCHAR", dealertypeid);
        //                            nv.Add("categoryid-INT", categoryid);
        //                            nv.Add("ownername-VARCHAR", owner_name == null ? "" : owner_name);
        //                            nv.Add("brand-VARCHAR", brand == null ? "" : brand);
        //                            nv.Add("dealercnic-VARCHAR", distributorcnic);
        //                            nv.Add("dealercontact-VARCHAR", distributorcontact);
        //                            nv.Add("area-VARCHAR", area);
        //                            nv.Add("zone-INT", zoneid);
        //                            nv.Add("city-INT", cityid);
        //                            //   nv.Add("group-VARCHAR", main_group);
        //                            nv.Add("address-VARCHAR", address);
        //                            nv.Add("monthofdealer-INT", month);
        //                            nv.Add("yearofdealer-INT", year);
        //                            isinserted = _DAL.InsertData("sp_insertuserdealer", nv, _DAL.CSManagementPortalDatabase);

        //                            if (!isinserted)
        //                            {
        //                                DataRow newRow = errordt.NewRow();
        //                                newRow.ItemArray = row.ItemArray.Clone() as object[];
        //                                newRow["errormessage"] = "Error occurred during inserting data";

        //                                errordt.Rows.Add(newRow);
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            DataRow newRow = errordt.NewRow();
        //                            newRow.ItemArray = row.ItemArray.Clone() as object[];
        //                            newRow["errormessage"] = "Error occurred during inserting data. Error Message: " + ex.Message;

        //                            errordt.Rows.Add(newRow);
        //                        }
        //                    }
        //                    else if (row["flag"].ToString() == "U")
        //                    {
        //                        // Handle update case if needed
        //                    }
        //                    else
        //                    {
        //                        //try
        //                        //{
        //                        //    nv.Clear();
        //                        //    nv.Add("userid-INT", userid);
        //                        //    nv.Add("dealerid-INT",dea);
        //                        //    nv.Add("monthofdealer-INT", month);
        //                        //    nv.Add("yearofdealer-INT", year);
        //                        //    isinserted = dAL.InsertData("sp_deleteuserdealer", nv, dAL.ConnectionMoltyDatabase);

        //                        //    if (!isinserted)
        //                        //    {
        //                        //        DataRow newRow = errordt.NewRow();
        //                        //        newRow.ItemArray = row.ItemArray.Clone() as object[];
        //                        //        newRow["errormessage"] = "Error occurred during deleting data";

        //                        //        errordt.Rows.Add(newRow);
        //                        //    }
        //                        //}
        //                        //catch (Exception ex)
        //                        //{
        //                        //    DataRow newRow = errordt.NewRow();
        //                        //    newRow.ItemArray = row.ItemArray.Clone() as object[];
        //                        //    newRow["errormessage"] = "Error occurred during deleting data. Error Message: " + ex.Message;

        //                        //    errordt.Rows.Add(newRow);
        //                        //}
        //                    }
        //                }
        //                else
        //                {

        //                    DataRow newRow = errordt.NewRow();
        //                    newRow.ItemArray = row.ItemArray.Clone() as object[];
        //                    newRow["errormessage"] = errormessage;

        //                    errordt.Rows.Add(newRow);

        //                }


        //            }

        //            errordt.AcceptChanges();

        //            return Ok(new { ErrorLines = errordt });
        //        }

        //        else
        //        {
        //            return BadRequest("Cannot read file, please confirm if you have uploaded an Excel file.");
        //        }


        //        //return Ok();

        //    }

        //}

        //#endregion




        //#region KPIQuestionnaireForm

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetKPIFoms()
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        dataTable = _DAL.GetData("sp_select_kpiform", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return Ok();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetKPIQuestion(string questionid)
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("questionid-INT", questionid);
        //        dataTable = _DAL.GetData("sp_select_kpiform_onid", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetDealerTypes()
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        dataTable = _DAL.GetData("sp_selectdealertypes", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetQuestionTypes()
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        dataTable = _DAL.GetData("sp_selectquestiontype", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetAnswerTypes()
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        dataTable = _DAL.GetData("sp_selectanswertype", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }




        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult InsertKPIForm([FromBody] KPIForm kPIForm)
        //{
        //    var claims = HttpContext.User.Claims;
        //    var UserID = (from c in claims where c.Type == "UserID" select c.Value).FirstOrDefault();


        //    NameValueCollection nv = new NameValueCollection();
        //    DataTable dt = new DataTable();
        //    using (SqlConnection connection = new SqlConnection(_DAL.CSManagementPortalDatabase))
        //    {
        //        connection.Open();

        //        using (SqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {

        //                //Check if KPI form exists for selected dealer type
        //                nv.Clear();
        //                nv.Add("dealer_type-INT", kPIForm.dealer_type);
        //                nv.Add("question_type-INT", kPIForm.question_type);
        //                dt = _DAL.GetData("sp_select_kpiformid", nv, _DAL.CSManagementPortalDatabase);
        //                var kpiformid = "";
        //                //if exists we will use already generated id of kpi form
        //                if (dt.Rows.Count > 0)
        //                {
        //                    kpiformid = dt.Rows[0]["id"].ToString();
        //                }
        //                //otherwise we will create a new form and get its form id
        //                else
        //                {
        //                    nv.Clear();
        //                    nv.Add("dealer_type-INT", kPIForm.dealer_type);
        //                    nv.Add("question_type-INT", kPIForm.question_type);
        //                    nv.Add("createby-INT", UserID.ToString());
        //                    dt.Clear();
        //                    dt = _DAL.GetData("sp_insert_kpiform", nv, connection, transaction);
        //                    kpiformid = dt.Rows[0]["id"].ToString();
        //                }

        //                dt.Clear();
        //                //Now we will enter question based on form id
        //                nv.Clear();
        //                nv.Add("kpiquestionsform_id-INT", kpiformid);
        //                nv.Add("question-varchar", kPIForm.question);
        //                nv.Add("createby-INT", UserID.ToString());
        //                nv.Add("remarks-bit", kPIForm.remarks.ToString());
        //                nv.Add("isactive-bit", kPIForm.status.ToString());
        //                nv.Add("guideline-varchar", kPIForm.guideline);
        //                dt = _DAL.GetData("sp_insert_kpiform_question", nv, connection, transaction);



        //                //Now we will enter answer types w.r.t question id and question needs to be inserted
        //                if (dt.Rows.Count > 0)
        //                {
        //                    var questionid = dt.Rows[0]["questionid"].ToString();

        //                    string[] answertypes = kPIForm.answertypes.Split(",");

        //                    foreach (var type in answertypes)
        //                    {
        //                        bool isinserted = false;
        //                        if (type == "2")
        //                        {
        //                            nv.Clear();
        //                            nv.Add("kpiquestionsform_id-INT", kpiformid);
        //                            nv.Add("questionid-int", questionid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", kPIForm.correctAnswer_yes);
        //                            isinserted = _DAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);


        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }
        //                        }
        //                        else if (type == "3")
        //                        {
        //                            nv.Clear();
        //                            nv.Add("kpiquestionsform_id-INT", kpiformid);
        //                            nv.Add("questionid-int", questionid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", kPIForm.correctAnswer_feed);
        //                            isinserted = _DAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);
        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }
        //                        }
        //                        else
        //                        {
        //                            nv.Clear();
        //                            nv.Add("kpiquestionsform_id-INT", kpiformid);
        //                            nv.Add("questionid-int", questionid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-int", "5");
        //                            isinserted = _DAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);
        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }
        //                        }
        //                    }

        //                    string[] attachments = kPIForm.attachments.Split(",");
        //                    //Now we will insert attachment types  w.r.t question id
        //                    bool attachmentInserted = false;
        //                    if (attachments.Length > 0)
        //                    {
        //                        nv.Clear();
        //                        nv.Add("kpiquestionsform_id-INT", kpiformid);
        //                        nv.Add("questionid-int", questionid);
        //                        nv.Add("createby-INT", UserID.ToString());
        //                        nv.Add("attachmenttypes-varchar", kPIForm.attachments);

        //                        attachmentInserted = _DAL.InsertData("sp_insert_kpiform_attachmenttype", nv, connection, transaction);
        //                    }
        //                    else
        //                    {
        //                        attachmentInserted = true;

        //                    }

        //                    if (attachmentInserted)
        //                    {
        //                        transaction.Commit();
        //                        return Ok();

        //                    }
        //                    else
        //                    {
        //                        transaction.Rollback();
        //                        return BadRequest("Error");
        //                    }
        //                }
        //                else
        //                {
        //                    transaction.Rollback();
        //                    BadRequest("Error");
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }
        //        }
        //    }

        //    return Ok();

        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult UpdateKPIForm([FromBody] KPIForm kPIForm)
        //{
        //    var claims = HttpContext.User.Claims;
        //    var UserID = (from c in claims where c.Type == "UserID" select c.Value).FirstOrDefault();


        //    NameValueCollection nv = new NameValueCollection();
        //    DataTable dt = new DataTable();
        //    using (SqlConnection connection = new SqlConnection(_DAL.CSManagementPortalDatabase))
        //    {
        //        connection.Open();

        //        using (SqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                bool isupdated = false;
        //                nv.Clear();
        //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                nv.Add("questionid-int", kPIForm.question_id);
        //                nv.Add("isactive-bit", kPIForm.status.ToString());
        //                isupdated = _DAL.InsertData("sp_update_kpiform_question_status", nv, connection, transaction);

        //                if (isupdated)
        //                {
        //                    return Ok();
        //                }
        //                else
        //                {
        //                    return BadRequest("Error");
        //                }
        //                #region  FOR NOW WE ARE ONLY ALLOWING STATUS TO UPDATE COMMENTING ALL CODE FOR FUTURE USE
        //                //First we will delete answer types ,attachment types then we will insert  them again
        //                //bool isdeleted = false;
        //                //nv.Clear();
        //                //nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //nv.Add("questionid-int", kPIForm.question_id);
        //                //isdeleted = dAL.InsertData("sp_delete_answertype_attachment_kpi",nv, connection, transaction);


        //                //if (isdeleted)
        //                //{
        //                //    nv.Clear();
        //                //    nv.Add("dealer_type-INT", kPIForm.dealer_type);
        //                //    nv.Add("userid-int", UserID.ToString());

        //                //    dt = dAL.GetData("sp_update_kpiform", nv, connection, transaction);

        //                //    if(dt.Rows.Count > 0)
        //                //    {
        //                //        dt.Clear();

        //                //        string[] answertypes = kPIForm.answertypes.Split(",");

        //                //        foreach (var type in answertypes)
        //                //        {
        //                //            bool isinserted = false;
        //                //            if (type == "2")
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-varchar", kPIForm.correctAnswer_yes);
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);


        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }
        //                //            }
        //                //            else if (type == "3")
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-varchar", kPIForm.correctAnswer_feed);
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);
        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }
        //                //            }
        //                //            else
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-int", "0");
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);
        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }

        //                //            }

        //                //        }

        //                //        //Now we will insert attachment types  w.r.t question id
        //                //        bool attachmentInserted = false;
        //                //        nv.Clear();
        //                //        nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //        nv.Add("questionid-int", kPIForm.question_id);
        //                //        nv.Add("createby-INT", UserID.ToString());
        //                //        nv.Add("attachmenttypes-varchar", kPIForm.attachments);

        //                //        attachmentInserted = dAL.InsertData("sp_insert_kpiform_attachmenttype", nv, connection, transaction);

        //                //        if (attachmentInserted)
        //                //        {
        //                //            transaction.Commit();
        //                //            return Ok();

        //                //        }
        //                //        else
        //                //        {
        //                //            transaction.Rollback();
        //                //            return BadRequest("Error");
        //                //        }
        //                //    }
        //                //    else
        //                //    {
        //                //        transaction.Rollback();
        //                //        return BadRequest("Error");
        //                //    }

        //                //}
        //                //else
        //                //{
        //                //    transaction.Rollback();
        //                //    return BadRequest("Error");
        //                //}
        //                #endregion

        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }

        //        }

        //    }

        //    //return Ok();

        //}


        //#endregion

        //#region OSForm

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetOSFoms()
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        dataTable = _DAL.GetData("sp_select_osform", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return Ok();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }




        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetOSBoard(string boardid)
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("boardid-INT", boardid);
        //        dataTable = _DAL.GetData("sp_select_osform_onid", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }




        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetCategories()
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        dataTable = _DAL.GetData("sp_selectcategorymaster", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return Ok();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }




        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult InsertOSForm([FromBody] OSForm osForm)
        //{
        //    var claims = HttpContext.User.Claims;
        //    var UserID = (from c in claims where c.Type == "UserID" select c.Value).FirstOrDefault();

        //    NameValueCollection nv = new NameValueCollection();
        //    DataTable dt = new DataTable();
        //    using (SqlConnection connection = new SqlConnection(_DAL.CSManagementPortalDatabase))
        //    {
        //        connection.Open();

        //        using (SqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                //Check if KPI form exists for selected dealer type
        //                nv.Clear();
        //                nv.Add("dealer_type-INT", osForm.dealer_type);
        //                dt = _DAL.GetData("sp_select_osformid", nv, _DAL.CSManagementPortalDatabase);
        //                var osformid = "";
        //                //if exists we will use already generated id of kpi form
        //                if (dt.Rows.Count > 0)
        //                {
        //                    osformid = dt.Rows[0]["id"].ToString();
        //                }
        //                //otherwise we will create a new form and get its form id
        //                else
        //                {
        //                    nv.Clear();
        //                    nv.Add("dealer_type-INT", osForm.dealer_type);
        //                    nv.Add("category_type-INT", osForm.category_type);
        //                    nv.Add("createby-INT", UserID.ToString());
        //                    dt.Clear();
        //                    dt = _DAL.GetData("sp_insert_osform", nv, connection, transaction);
        //                    osformid = dt.Rows[0]["id"].ToString();

        //                }


        //                dt.Clear();
        //                //Now we will enter question based on form id
        //                nv.Clear();
        //                nv.Add("osform_id-INT", osformid);
        //                nv.Add("boarddescription-varchar", osForm.boarddescription);
        //                nv.Add("category_type-INT", osForm.category_type);
        //                nv.Add("createby-INT", UserID.ToString());
        //                nv.Add("remarks-bit", osForm.remarks.ToString());
        //                nv.Add("guideline-varchar", osForm.guideline);
        //                nv.Add("isactive-varchar", osForm.status.ToString());
        //                dt = _DAL.GetData("sp_insert_osform_board", nv, connection, transaction);



        //                //Now we will enter answer types w.r.t question id and question needs to be inserted
        //                if (dt.Rows.Count > 0)
        //                {
        //                    var boardid = dt.Rows[0]["boardid"].ToString();

        //                    string[] answertypes = osForm.answertypes.Split(",");

        //                    foreach (var type in answertypes)
        //                    {
        //                        bool isinserted = false;
        //                        if (type == "2")
        //                        {
        //                            nv.Clear();
        //                            nv.Add("osform_id-INT", osformid);
        //                            nv.Add("baordid-int", boardid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", osForm.correctAnswer_yes);
        //                            isinserted = _DAL.InsertData("sp_insert_osform_answertype", nv, connection, transaction);


        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }
        //                        }
        //                        else if (type == "3")
        //                        {
        //                            nv.Clear();
        //                            nv.Add("osform_id-INT", osformid);
        //                            nv.Add("baordid-int", boardid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", osForm.correctAnswer_feed);
        //                            isinserted = _DAL.InsertData("sp_insert_osform_answertype", nv, connection, transaction);
        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }
        //                        }
        //                        else
        //                        {
        //                            nv.Clear();
        //                            nv.Add("osform_id-INT", osformid);
        //                            nv.Add("baordid-int", boardid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", "0");
        //                            isinserted = _DAL.InsertData("sp_insert_osform_answertype", nv, connection, transaction);
        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }

        //                        }

        //                    }

        //                    //Now we will insert attachment types  w.r.t question id
        //                    bool attachmentInserted = false;
        //                    string[] attachments = osForm.attachments.Split(",");

        //                    if (attachments.Length > 0)
        //                    {

        //                        nv.Clear();
        //                        nv.Add("osform_id-INT", osformid);
        //                        nv.Add("boardid-int", boardid);
        //                        nv.Add("createby-INT", UserID.ToString());
        //                        nv.Add("attachmenttypes-varchar", osForm.attachments);

        //                        attachmentInserted = _DAL.InsertData("sp_insert_osform_attachmenttype", nv, connection, transaction);
        //                    }
        //                    else
        //                    {
        //                        attachmentInserted = true;
        //                    }

        //                    if (attachmentInserted)
        //                    {
        //                        bool fileuploaded = false;

        //                        nv.Clear();
        //                        nv.Add("formid-INT", osformid);
        //                        nv.Add("boardid-int", boardid);
        //                        nv.Add("filepaths-VARCHAR", osForm.filepaths);
        //                        nv.Add("createby-INT", UserID.ToString());

        //                        fileuploaded = _DAL.InsertData("sp_upload_images_osform", nv, connection, transaction);

        //                        if (fileuploaded)
        //                        {
        //                            transaction.Commit();
        //                            return Ok();
        //                        }
        //                        else
        //                        {
        //                            transaction.Rollback();
        //                            return BadRequest("Error");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        transaction.Rollback();
        //                        return BadRequest("Error");
        //                    }
        //                }
        //                else
        //                {
        //                    transaction.Rollback();
        //                    BadRequest("Error");
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }

        //        }

        //    }

        //    return Ok();

        //}

        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult UpdateOSForm([FromBody] OSForm oSForm)
        //{
        //    var claims = HttpContext.User.Claims;
        //    var UserID = (from c in claims where c.Type == "UserID" select c.Value).FirstOrDefault();


        //    NameValueCollection nv = new NameValueCollection();
        //    DataTable dt = new DataTable();
        //    using (SqlConnection connection = new SqlConnection(_DAL.CSManagementPortalDatabase))
        //    {
        //        connection.Open();

        //        using (SqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                bool isupdated = false;
        //                nv.Clear();
        //                nv.Add("osform_id-INT", oSForm.form_id);
        //                nv.Add("boardid-int", oSForm.board_id);
        //                nv.Add("isactive-bit", oSForm.status.ToString());
        //                nv.Add("category_type-int", oSForm.category_type);
        //                isupdated = _DAL.InsertData("sp_update_osform_question_status", nv, connection, transaction);

        //                if (isupdated)
        //                {
        //                    transaction.Commit();
        //                    return Ok();
        //                }
        //                else
        //                {
        //                    return BadRequest("Error");
        //                }

        //                #region  FOR NOW WE ARE ONLY ALLOWING STATUS TO UPDATE COMMENTING ALL CODE FOR FUTURE USE


        //                //##### just replace kpi procedure to OS procedure First #####

        //                //First we will delete answer types ,attachment types then we will insert  them again
        //                //bool isdeleted = false;
        //                //nv.Clear();
        //                //nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //nv.Add("questionid-int", kPIForm.question_id);
        //                //isdeleted = dAL.InsertData("sp_delete_answertype_attachment_kpi",nv, connection, transaction);


        //                //if (isdeleted)
        //                //{
        //                //    nv.Clear();
        //                //    nv.Add("dealer_type-INT", kPIForm.dealer_type);
        //                //    nv.Add("userid-int", UserID.ToString());

        //                //    dt = dAL.GetData("sp_update_kpiform", nv, connection, transaction);

        //                //    if(dt.Rows.Count > 0)
        //                //    {
        //                //        dt.Clear();

        //                //        string[] answertypes = kPIForm.answertypes.Split(",");

        //                //        foreach (var type in answertypes)
        //                //        {
        //                //            bool isinserted = false;
        //                //            if (type == "2")
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-varchar", kPIForm.correctAnswer_yes);
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);


        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }
        //                //            }
        //                //            else if (type == "3")
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-varchar", kPIForm.correctAnswer_feed);
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);
        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }
        //                //            }
        //                //            else
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-int", "0");
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);
        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }

        //                //            }

        //                //        }

        //                //        //Now we will insert attachment types  w.r.t question id
        //                //        bool attachmentInserted = false;
        //                //        nv.Clear();
        //                //        nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //        nv.Add("questionid-int", kPIForm.question_id);
        //                //        nv.Add("createby-INT", UserID.ToString());
        //                //        nv.Add("attachmenttypes-varchar", kPIForm.attachments);

        //                //        attachmentInserted = dAL.InsertData("sp_insert_kpiform_attachmenttype", nv, connection, transaction);

        //                //        if (attachmentInserted)
        //                //        {
        //                //            transaction.Commit();
        //                //            return Ok();

        //                //        }
        //                //        else
        //                //        {
        //                //            transaction.Rollback();
        //                //            return BadRequest("Error");
        //                //        }
        //                //    }
        //                //    else
        //                //    {
        //                //        transaction.Rollback();
        //                //        return BadRequest("Error");
        //                //    }

        //                //}
        //                //else
        //                //{
        //                //    transaction.Rollback();
        //                //    return BadRequest("Error");
        //                //}
        //                #endregion

        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }

        //        }

        //    }

        //    //return Ok();


        //}

        //#endregion

        //#region CWMForm

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetCWMFoms()
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        dataTable = _DAL.GetData("sp_select_cwmform", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return Ok();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }




        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetCWMModel(string modelid)
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("modelid-INT", modelid);
        //        dataTable = _DAL.GetData("sp_select_cwmform_onid", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }

        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult UploadImage([FromBody] CWMForm cWMForm)
        //{
        //    var claims = HttpContext.User.Claims;
        //    var UserID = (from c in claims where c.Type == "UserID" select c.Value).FirstOrDefault();

        //    NameValueCollection nv = new NameValueCollection();
        //    bool imageinserted = false;

        //    nv.Clear();
        //    nv.Add("category_type-INT", cWMForm.category_type);
        //    nv.Add("filepath-VARCHAR", cWMForm.FilePath);
        //    nv.Add("userid-INT", UserID.ToString());

        //    imageinserted = _DAL.InsertData("sp_uploadimage_cwmform", nv, _DAL.CSManagementPortalDatabase);

        //    if (imageinserted)
        //    {
        //        return Ok();
        //    }
        //    else
        //    {
        //        return BadRequest("Error");
        //    }

        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult InsertCWMForm([FromBody] CWMForm cWMForm)
        //{
        //    var claims = HttpContext.User.Claims;
        //    var UserID = (from c in claims where c.Type == "UserID" select c.Value).FirstOrDefault();

        //    NameValueCollection nv = new NameValueCollection();
        //    DataTable dt = new DataTable();
        //    using (SqlConnection connection = new SqlConnection(_DAL.CSManagementPortalDatabase))
        //    {
        //        connection.Open();

        //        using (SqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                //Check if KPI form exists for selected dealer type
        //                nv.Clear();
        //                nv.Add("category_type-INT", cWMForm.category_type);
        //                dt = _DAL.GetData("sp_select_cwmformid", nv, _DAL.CSManagementPortalDatabase);
        //                var cwmformid = "";
        //                //if exists we will use already generated id of kpi form
        //                if (dt.Rows.Count > 0)
        //                {
        //                    cwmformid = dt.Rows[0]["id"].ToString();
        //                }
        //                //otherwise we will create a new form and get its form id
        //                else
        //                {
        //                    nv.Clear();
        //                    nv.Add("category_type-INT", cWMForm.category_type);
        //                    nv.Add("createby-INT", UserID.ToString());
        //                    dt.Clear();
        //                    dt = _DAL.GetData("sp_insert_cwmform", nv, connection, transaction);
        //                    cwmformid = dt.Rows[0]["id"].ToString();

        //                }


        //                dt.Clear();
        //                //Now we will enter question based on form id
        //                nv.Clear();
        //                nv.Add("cwmform_id-INT", cwmformid);
        //                nv.Add("category_type-INT", cWMForm.category_type);
        //                nv.Add("company_standard-varchar", cWMForm.companystandard);
        //                nv.Add("standard-varchar", cWMForm.standard);
        //                nv.Add("createby-INT", UserID.ToString());
        //                nv.Add("remarks-bit", cWMForm.remarks.ToString());
        //                nv.Add("guideline-varchar", cWMForm.guideline);
        //                nv.Add("isactive-varchar", cWMForm.status.ToString());
        //                dt = _DAL.GetData("sp_insert_cwmform_model", nv, connection, transaction);



        //                //Now we will enter answer types w.r.t question id and question needs to be inserted
        //                if (dt.Rows.Count > 0)
        //                {
        //                    var modelid = dt.Rows[0]["modelid"].ToString();


        //                    string[] answertypes = cWMForm.answertypes.Split(",");

        //                    foreach (var type in answertypes)
        //                    {
        //                        bool isinserted = false;
        //                        if (type == "2")
        //                        {
        //                            nv.Clear();
        //                            nv.Add("cwmform_id-INT", cwmformid);
        //                            nv.Add("modelid-int", modelid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", cWMForm.correctAnswer_yes);
        //                            isinserted = _DAL.InsertData("sp_insert_cwmform_answertype", nv, connection, transaction);


        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }
        //                        }
        //                        else if (type == "3")
        //                        {
        //                            nv.Clear();
        //                            nv.Add("cwmform_id-INT", cwmformid);
        //                            nv.Add("modelid-int", modelid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", cWMForm.correctAnswer_feed);
        //                            isinserted = _DAL.InsertData("sp_insert_cwmform_answertype", nv, connection, transaction);

        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }
        //                        }
        //                        else
        //                        {
        //                            nv.Clear();
        //                            nv.Add("cwmform_id-INT", cwmformid);
        //                            nv.Add("modelid-int", modelid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", "0");
        //                            isinserted = _DAL.InsertData("sp_insert_cwmform_answertype", nv, connection, transaction);

        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }

        //                        }

        //                    }
        //                    string[] attachments = cWMForm.attachments.Split(",");

        //                    //Now we will insert attachment types  w.r.t question id
        //                    bool attachmentInserted = false;
        //                    if (attachments.Length > 0)
        //                    {
        //                        nv.Clear();
        //                        nv.Add("cwmform_id-INT", cwmformid);
        //                        nv.Add("modelid-int", modelid);
        //                        nv.Add("createby-INT", UserID.ToString());
        //                        nv.Add("attachmenttypes-varchar", cWMForm.attachments);

        //                        attachmentInserted = _DAL.InsertData("sp_insert_cwmform_attachmenttype", nv, connection, transaction);
        //                    }
        //                    else
        //                    {
        //                        attachmentInserted = true;
        //                    }
        //                    if (attachmentInserted)
        //                    {
        //                        transaction.Commit();
        //                        return Ok();
        //                    }
        //                    else
        //                    {
        //                        transaction.Rollback();
        //                        return BadRequest("Error");
        //                    }
        //                }
        //                else
        //                {
        //                    transaction.Rollback();
        //                    BadRequest("Error");
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }

        //        }

        //    }

        //    return Ok();


        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult UpdateCWMForm([FromBody] CWMForm cWMForm)
        //{
        //    var claims = HttpContext.User.Claims;
        //    var UserID = (from c in claims where c.Type == "UserID" select c.Value).FirstOrDefault();


        //    NameValueCollection nv = new NameValueCollection();
        //    DataTable dt = new DataTable();
        //    using (SqlConnection connection = new SqlConnection(_DAL.CSManagementPortalDatabase))
        //    {
        //        connection.Open();

        //        using (SqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                bool isupdated = false;
        //                nv.Clear();
        //                nv.Add("cwmform_id-INT", cWMForm.form_id);
        //                nv.Add("modelid-int", cWMForm.model_id);
        //                nv.Add("isactive-bit", cWMForm.status.ToString());
        //                nv.Add("category_type-int", cWMForm.category_type);
        //                isupdated = _DAL.InsertData("sp_update_cwmform_model_status", nv, connection, transaction);

        //                if (isupdated)
        //                {
        //                    transaction.Commit();
        //                    return Ok();
        //                }
        //                else
        //                {
        //                    return BadRequest("Error");
        //                }

        //                #region  FOR NOW WE ARE ONLY ALLOWING STATUS TO UPDATE COMMENTING ALL CODE FOR FUTURE USE


        //                //##### just replace kpi procedure to OS procedure First #####

        //                //First we will delete answer types ,attachment types then we will insert  them again
        //                //bool isdeleted = false;
        //                //nv.Clear();
        //                //nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //nv.Add("questionid-int", kPIForm.question_id);
        //                //isdeleted = dAL.InsertData("sp_delete_answertype_attachment_kpi",nv, connection, transaction);


        //                //if (isdeleted)
        //                //{
        //                //    nv.Clear();
        //                //    nv.Add("dealer_type-INT", kPIForm.dealer_type);
        //                //    nv.Add("userid-int", UserID.ToString());

        //                //    dt = dAL.GetData("sp_update_kpiform", nv, connection, transaction);

        //                //    if(dt.Rows.Count > 0)
        //                //    {
        //                //        dt.Clear();

        //                //        string[] answertypes = kPIForm.answertypes.Split(",");

        //                //        foreach (var type in answertypes)
        //                //        {
        //                //            bool isinserted = false;
        //                //            if (type == "2")
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-varchar", kPIForm.correctAnswer_yes);
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);


        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }
        //                //            }
        //                //            else if (type == "3")
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-varchar", kPIForm.correctAnswer_feed);
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);
        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }
        //                //            }
        //                //            else
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-int", "0");
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);
        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }

        //                //            }

        //                //        }

        //                //        //Now we will insert attachment types  w.r.t question id
        //                //        bool attachmentInserted = false;
        //                //        nv.Clear();
        //                //        nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //        nv.Add("questionid-int", kPIForm.question_id);
        //                //        nv.Add("createby-INT", UserID.ToString());
        //                //        nv.Add("attachmenttypes-varchar", kPIForm.attachments);

        //                //        attachmentInserted = dAL.InsertData("sp_insert_kpiform_attachmenttype", nv, connection, transaction);

        //                //        if (attachmentInserted)
        //                //        {
        //                //            transaction.Commit();
        //                //            return Ok();

        //                //        }
        //                //        else
        //                //        {
        //                //            transaction.Rollback();
        //                //            return BadRequest("Error");
        //                //        }
        //                //    }
        //                //    else
        //                //    {
        //                //        transaction.Rollback();
        //                //        return BadRequest("Error");
        //                //    }

        //                //}
        //                //else
        //                //{
        //                //    transaction.Rollback();
        //                //    return BadRequest("Error");
        //                //}
        //                #endregion

        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }

        //        }
        //    }

        //    //return Ok();

        //}


        //#endregion

        //#region DFQForm

        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetDFQFoms()
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        dataTable = _DAL.GetData("sp_select_dfqform", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return Ok();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpGet]
        //public IActionResult GetDFQQuestion(string questionid)
        //{
        //    DataTable dataTable = new DataTable();
        //    try
        //    {
        //        NameValueCollection nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("questionid-INT", questionid);
        //        dataTable = _DAL.GetData("sp_select_dfqform_onid", nv, _DAL.CSManagementPortalDatabase);

        //        nv = null;

        //        if (dataTable.Rows.Count > 0)
        //        {
        //            return Ok(dataTable);
        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult InsertDFQForm([FromBody] DFQForm dFQForm)
        //{
        //    var claims = HttpContext.User.Claims;
        //    var UserID = (from c in claims where c.Type == "UserID" select c.Value).FirstOrDefault();


        //    NameValueCollection nv = new NameValueCollection();
        //    DataTable dt = new DataTable();
        //    using (SqlConnection connection = new SqlConnection(_DAL.CSManagementPortalDatabase))
        //    {
        //        connection.Open();

        //        using (SqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {

        //                //Check if KPI form exists for selected dealer type
        //                nv.Clear();
        //                dt = _DAL.GetData("sp_select_dfqformid", nv, _DAL.CSManagementPortalDatabase);
        //                var dfqformid = "";
        //                //if exists we will use already generated id of kpi form
        //                if (dt.Rows.Count > 0)
        //                {
        //                    dfqformid = dt.Rows[0]["id"].ToString();


        //                }
        //                //otherwise we will create a new form and get its form id
        //                else
        //                {
        //                    nv.Clear();
        //                    nv.Add("createby-INT", UserID.ToString());
        //                    dt.Clear();
        //                    dt = _DAL.GetData("sp_insert_dfqform", nv, connection, transaction);
        //                    dfqformid = dt.Rows[0]["id"].ToString();

        //                }


        //                dt.Clear();
        //                //Now we will enter question based on form id
        //                nv.Clear();
        //                nv.Add("dfqformid-INT", dfqformid);
        //                nv.Add("question-INT", dFQForm.Question);
        //                nv.Add("createby-INT", UserID.ToString());
        //                nv.Add("remarks-bit", dFQForm.remarks.ToString());
        //                nv.Add("guideline-varchar", dFQForm.guideline);
        //                nv.Add("isactive-varchar", dFQForm.status.ToString());
        //                dt = _DAL.GetData("sp_insert_dfqform_question", nv, connection, transaction);



        //                //Now we will enter answer types w.r.t question id and question needs to be inserted
        //                if (dt.Rows.Count > 0)
        //                {
        //                    var questionid = dt.Rows[0]["questionid"].ToString();


        //                    string[] answertypes = dFQForm.answertypes.Split(",");

        //                    foreach (var type in answertypes)
        //                    {
        //                        bool isinserted = false;
        //                        if (type == "2")
        //                        {
        //                            nv.Clear();
        //                            nv.Add("dfqformid-INT", dfqformid);
        //                            nv.Add("questionid-int", questionid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", dFQForm.correctAnswer_yes);
        //                            isinserted = _DAL.InsertData("sp_insert_dfqform_answertype", nv, connection, transaction);


        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }
        //                        }
        //                        else if (type == "3")
        //                        {
        //                            nv.Clear();
        //                            nv.Add("dfqformid-INT", dfqformid);
        //                            nv.Add("questionid-int", questionid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", dFQForm.correctAnswer_feed);
        //                            isinserted = _DAL.InsertData("sp_insert_dfqform_answertype", nv, connection, transaction);

        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }
        //                        }
        //                        else
        //                        {
        //                            nv.Clear();
        //                            nv.Add("dfqformid-INT", dfqformid);
        //                            nv.Add("questionid-int", questionid);
        //                            nv.Add("createby-INT", UserID.ToString());
        //                            nv.Add("answertype-varchar", type);
        //                            nv.Add("correctanswer-varchar", "0");
        //                            isinserted = _DAL.InsertData("sp_insert_dfqform_answertype", nv, connection, transaction);

        //                            if (!isinserted)
        //                            {
        //                                transaction.Rollback();
        //                                return BadRequest("Error");
        //                            }

        //                        }

        //                    }

        //                    string[] attachments = dFQForm.attachments.Split(",");

        //                    //Now we will insert attachment types  w.r.t question id
        //                    bool attachmentInserted = false;
        //                    if (attachments.Length > 0)
        //                    {
        //                        nv.Clear();
        //                        nv.Add("dfqform_id-INT", dfqformid);
        //                        nv.Add("questionid-int", questionid);
        //                        nv.Add("createby-INT", UserID.ToString());
        //                        nv.Add("attachmenttypes-varchar", dFQForm.attachments);

        //                        attachmentInserted = _DAL.InsertData("sp_insert_dfqform_attachmenttype", nv, connection, transaction);
        //                    }
        //                    else
        //                    {
        //                        attachmentInserted = true;
        //                    }
        //                    if (attachmentInserted)
        //                    {
        //                        transaction.Commit();
        //                        return Ok();
        //                    }
        //                    else
        //                    {
        //                        transaction.Rollback();
        //                        return BadRequest("Error");
        //                    }
        //                }
        //                else
        //                {
        //                    transaction.Rollback();
        //                    BadRequest("Error");
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }

        //        }

        //    }

        //    return Ok();


        //}


        //[RateLimitMiddleware(50, 5)]
        //[HttpPost]
        //public IActionResult UpdateDFQForm([FromBody] DFQForm dFQForm)
        //{
        //    var claims = HttpContext.User.Claims;
        //    var UserID = (from c in claims where c.Type == "UserID" select c.Value).FirstOrDefault();


        //    NameValueCollection nv = new NameValueCollection();
        //    DataTable dt = new DataTable();
        //    using (SqlConnection connection = new SqlConnection(_DAL.CSManagementPortalDatabase))
        //    {
        //        connection.Open();

        //        using (SqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                bool isupdated = false;
        //                nv.Clear();
        //                nv.Add("dfqform_id-INT", dFQForm.form_id);
        //                nv.Add("questionid-int", dFQForm.questionid);
        //                nv.Add("isactive-bit", dFQForm.status.ToString());
        //                isupdated = _DAL.InsertData("sp_update_dfqform_question_status", nv, connection, transaction);

        //                if (isupdated)
        //                {
        //                    transaction.Commit();
        //                    return Ok();
        //                }
        //                else
        //                {
        //                    return BadRequest("Error");
        //                }

        //                #region  FOR NOW WE ARE ONLY ALLOWING STATUS TO UPDATE COMMENTING ALL CODE FOR FUTURE USE
        //                //##### just replace kpi procedure to OS procedure First #####

        //                //First we will delete answer types ,attachment types then we will insert  them again
        //                //bool isdeleted = false;
        //                //nv.Clear();
        //                //nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //nv.Add("questionid-int", kPIForm.question_id);
        //                //isdeleted = dAL.InsertData("sp_delete_answertype_attachment_kpi",nv, connection, transaction);


        //                //if (isdeleted)
        //                //{
        //                //    nv.Clear();
        //                //    nv.Add("dealer_type-INT", kPIForm.dealer_type);
        //                //    nv.Add("userid-int", UserID.ToString());

        //                //    dt = dAL.GetData("sp_update_kpiform", nv, connection, transaction);

        //                //    if(dt.Rows.Count > 0)
        //                //    {
        //                //        dt.Clear();

        //                //        string[] answertypes = kPIForm.answertypes.Split(",");

        //                //        foreach (var type in answertypes)
        //                //        {
        //                //            bool isinserted = false;
        //                //            if (type == "2")
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-varchar", kPIForm.correctAnswer_yes);
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);


        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }
        //                //            }
        //                //            else if (type == "3")
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-varchar", kPIForm.correctAnswer_feed);
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);
        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }
        //                //            }
        //                //            else
        //                //            {
        //                //                nv.Clear();
        //                //                nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //                nv.Add("questionid-int", kPIForm.question_id);
        //                //                nv.Add("createby-INT", UserID.ToString());
        //                //                nv.Add("answertype-varchar", type);
        //                //                nv.Add("correctanswer-int", "0");
        //                //                isinserted = dAL.InsertData("sp_insert_kpiform_answertype", nv, connection, transaction);
        //                //                if (!isinserted)
        //                //                {
        //                //                    transaction.Rollback();
        //                //                    return BadRequest("Error");
        //                //                }

        //                //            }

        //                //        }

        //                //        //Now we will insert attachment types  w.r.t question id
        //                //        bool attachmentInserted = false;
        //                //        nv.Clear();
        //                //        nv.Add("kpiquestionsform_id-INT", kPIForm.form_id);
        //                //        nv.Add("questionid-int", kPIForm.question_id);
        //                //        nv.Add("createby-INT", UserID.ToString());
        //                //        nv.Add("attachmenttypes-varchar", kPIForm.attachments);

        //                //        attachmentInserted = dAL.InsertData("sp_insert_kpiform_attachmenttype", nv, connection, transaction);

        //                //        if (attachmentInserted)
        //                //        {
        //                //            transaction.Commit();
        //                //            return Ok();

        //                //        }
        //                //        else
        //                //        {
        //                //            transaction.Rollback();
        //                //            return BadRequest("Error");
        //                //        }
        //                //    }
        //                //    else
        //                //    {
        //                //        transaction.Rollback();
        //                //        return BadRequest("Error");
        //                //    }

        //                //}
        //                //else
        //                //{
        //                //    transaction.Rollback();
        //                //    return BadRequest("Error");
        //                //}
        //                #endregion


        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }

        //        }

        //    }

        //    //return Ok();


        //}

        //#endregion


        //#endregion


    }
}
