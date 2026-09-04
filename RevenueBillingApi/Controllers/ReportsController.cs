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
    public class ReportsController : ControllerBase
    {
        private readonly DataAccessLayer _DAL;
        private readonly SendEmail _sendemail;
        private readonly ILogger<ReportsController> _logger;
        private readonly DataEncryptor _dataencryptor;
        private readonly RandomStringGenerator _randomstringgenerator;
        private readonly CommonMethods _CommonMethods;

        public ReportsController(DataAccessLayer DAL, ILogger<ReportsController> logger, SendEmail sendemail, DataEncryptor dataencryptor, RandomStringGenerator randomstringgenerator, CommonMethods commonMethods)
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

        #region Reports

        #region Account_VoucherSystem



        #endregion

        #region Payable / Purchase Module


        #endregion

        #region Receivable / Sale Module


        #endregion




        #endregion


    }
}
