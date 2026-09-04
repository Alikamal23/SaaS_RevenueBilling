using RevenueBillingApi.Context;
using RevenueBillingApi.Extensions;
using RevenueBillingApi.Models.Authentication;
using RevenueBillingApi.Models.Settings;
using RevenueBillingApi.RateLimiting;
using RevenueBillingApi.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Data;
using System.DirectoryServices;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace RevenueBillingApi.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/{controller}/{action}/{id:int?}")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly DataAccessLayer _DAL;
        private readonly SendEmail _sendemail;
        private readonly ILogger<SettingsController> _logger;
        private readonly DataEncryptor _dataencryptor;
        private readonly RandomStringGenerator _randomstringgenerator;
        private readonly CommonMethods _CommonMethods;
        public SettingsController(DataAccessLayer DAL,ILogger<SettingsController> logger, SendEmail sendemail, DataEncryptor dataencryptor, RandomStringGenerator randomstringgenerator, CommonMethods commonMethods)
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
   
        #region RolesMaster
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetRoles()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_roles", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_roles");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_roles");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetRolesByID(string RoleID)
        {
            DataTable dt = new DataTable();
            string methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("RoleID-INT", RoleID);
                dt = _DAL.GetData("sp_select_roles_byid", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_roles_byid");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_roles_byid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditRoles([FromBody] Role role)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("RoleID-INT", role.RoleID);
                nv.Add("RoleName-VARCHAR", HttpUtility.HtmlEncode(role.RoleName));
                nv.Add("IsActive-BIT", role.IsActive == null ? "0" : "1");
                nv.Add("UserID-INT", role.UserID);
                Result = _DAL.InsertData("sp_update_role", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_update_role");

                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_update_role");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
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
        public IActionResult SaveRoles([FromBody] Role role)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("RoleName-VARCHAR", HttpUtility.HtmlEncode(role.RoleName));
                nv.Add("IsActive-BIT", role.IsActive == null ? "0" : "1");
                nv.Add("UserID-INT", role.UserID);
                Result = _DAL.InsertData("sp_insert_role", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_role");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_role");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
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
        public IActionResult DeleteRoles([FromBody] DeleteFromDB deleteFromDB_)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("RoleID-INT", deleteFromDB_.RoleID);
                nv.Add("UserID-INT", deleteFromDB_.UserID);
                Result = _DAL.InsertData("sp_delete_role", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_delete_role");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete2 + "sp_delete_role");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            return Ok(Result);
        }
        #endregion

        #region UserMaster
        //[RateLimitMiddleware(100, 5)]
        //[HttpGet]
        //public IActionResult GetUserType(string TypeID)
        //{
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        NameValueCollection? nv = new NameValueCollection();
        //        nv.Clear();
        //        nv.Add("TypeID-INT", TypeID);
        //        dt = _DAL.GetData("sp_select_usertype", nv, _DAL.CSManagementPortalDatabase);

        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_usertype");
        //        }
        //        else
        //        {
        //            SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_usertype");
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

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetAllCities()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                dt = _DAL.GetData("sp_select_cities", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_cities");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_cities");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            return Ok(dt);

        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetUsersByType(string UserID)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("UserID-INT", UserID);
                dt = _DAL.GetData("sp_select_users", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_users");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_users");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " sp_select_users" + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetForms()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_forms", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_forms");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_forms");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditUsers([FromBody] User user)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("UserID-INT", user.UserID);
                nv.Add("UserName-VARCHAR", HttpUtility.HtmlEncode(user.UserName));
                nv.Add("UserEmail-VARCHAR", HttpUtility.HtmlEncode(user.UserEmail));
                nv.Add("RoleID-INT", user.RoleID);
                nv.Add("DefaultFormID-INT", user.FormID);
                nv.Add("UserTypeID-INT", user.UserTypeID);
                //nv.Add("CityID-INT", user.CityID);
                nv.Add("MobileNumber-VARCHAR", HttpUtility.HtmlEncode(user.MobileNumber));
                nv.Add("IsActive-BIT", user.IsActive == null ? "0" : "1");
                nv.Add("EditUserID-INT", user.EditUserID);
                nv.Add("IsAdUser-BIT", user.IsAdUser == null ? "0" : "1");
                nv.Add("Designation-VARCHAR", user.Designation == null ? "NULL" : HttpUtility.HtmlEncode(user.Designation));
                nv.Add("Department-VARCHAR", user.Department == null ? "NULL" : HttpUtility.HtmlEncode(user.Department));
                nv.Add("DomainName-VARCHAR", user.DomainName == null ? "NULL" : HttpUtility.HtmlEncode(user.DomainName));
                nv.Add("LoginName-VARCHAR", HttpUtility.HtmlEncode(user.LoginName));
                Result = _DAL.InsertData("sp_update_user", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_update_user");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_update_user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
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
        public IActionResult SaveUsers([FromBody] User user)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                
                string encrypted_password = string.Empty;
                string isAdUser = user.IsAdUser == null ? "0" : "1";
                if (isAdUser == "0")
                {
                    string RandomStrings = _randomstringgenerator.GetRandomString();
                    string DefaultPwd = "Abcd@12345";

                    ////string EmailBody = ActivityLog.EmailBody + " RandomPassword : " + RandomStrings;

                    // HTML template mein placeholder ko replace karo
                    string EmailBody = ActivityLog.EmailBody.Replace("{{PASSWORD}}", RandomStrings);
                    bool SendMail = _sendemail.SendEmailToUsers(new List<string> { user.UserEmail }, EmailBody, ActivityLog.EmailSubject, _DAL);

                    if (SendMail) {
                        encrypted_password = _CommonMethods.HashPassword(RandomStrings);
                    } else {
                        encrypted_password = _CommonMethods.HashPassword(DefaultPwd);
                    }
                }


                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("UserID-INT", (user.UserID == null ? "NULL" : user.UserID));
                nv.Add("UserName-VARCHAR", HttpUtility.HtmlEncode(user.UserName));
                nv.Add("UserEmail-VARCHAR", (user.UserEmail == null ? "NULL" : HttpUtility.HtmlEncode(user.UserEmail)));
                nv.Add("UserPassword-VARCHAR", encrypted_password);
                nv.Add("RoleID-INT", user.RoleID);
                nv.Add("DefaultFormID-INT", user.FormID);
                nv.Add("UserTypeID-INT", user.UserTypeID);
                //nv.Add("CityID-INT", user.CityID);
                nv.Add("MobileNumber-VARCHAR", HttpUtility.HtmlEncode(user.MobileNumber));
                nv.Add("IsActive-INT", user.IsActive == null ? "0" : "1");
                nv.Add("CreateUserID-INT", user.EditUserID);
                nv.Add("IsAdUser-BIT", user.IsAdUser == null ? "0" : "1");
                nv.Add("Designation-VARCHAR", user.Designation == null ? "NULL" : HttpUtility.HtmlEncode(user.Designation));
                nv.Add("Department-VARCHAR", user.Department == null ? "NULL" : HttpUtility.HtmlEncode(user.Department));
                nv.Add("DomainName-VARCHAR", user.DomainName == null ? "NULL" : HttpUtility.HtmlEncode(user.DomainName));
                nv.Add("LoginName-VARCHAR", HttpUtility.HtmlEncode(user.LoginName));
                dt = _DAL.GetData("sp_insert_user", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_user");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_user");
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

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteUsers([FromBody] DeleteFromDB deleteFromDB_)
        {
            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("UserID-INT", deleteFromDB_.UserID);
                nv.Add("EditUserID-INT", deleteFromDB_.EditUserID);
                Result = _DAL.InsertData("sp_delete_user", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_delete_user");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete2 + "sp_delete_user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            return Ok(Result);
        }

        #endregion

        #region RolesMapping

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetMapping(string RoleId)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();

                nv.Clear();
                nv.Add("RoleID-INT", RoleId);
                dt = _DAL.GetData("sp_select_rolesmapping", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_rolesmapping");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_rolesmapping");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetMapping_2(string RoleId)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();

                nv.Clear();
                nv.Add("RoleID-INT", RoleId);
                dt = _DAL.GetData("sp_select_rolesmapping_2", nv, _DAL.CSManagementPortalDatabase);
                nv = null;
                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_rolesmapping_2");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_rolesmapping_2");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult UpdateRolesMapping([FromBody] RoleMapping mapping)
        {
            bool Result = false;
            bool Result2 = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();

                nv.Clear();
                nv.Add("RoleID-INT", mapping.RoleID);
                Result = _DAL.InsertData("sp_delete_rolemapping", nv, _DAL.CSManagementPortalDatabase);

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete + "sp_delete_rolemapping");

                    foreach (RoleMappingCollection item in mapping.RoleMappingCollection)
                    {
                        nv.Clear();
                        nv.Add("RoleID-INT", mapping.RoleID);
                        nv.Add("UserID-INT", mapping.UserID);
                        nv.Add("FormsID-INT", item.FormsID);
                        nv.Add("AllowInsert-BIT", item.AllowInsert);
                        nv.Add("AllowUpdate-BIT", item.AllowUpdate);
                        nv.Add("AllowDelete-BIT", item.AllowDelete);
                        Result2 = _DAL.InsertData("sp_update_rolesmapping", nv, _DAL.CSManagementPortalDatabase);

                        if (Result2)
                        {
                            SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_update_rolesmapping");
                        }
                        else
                        {
                            SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_update_rolesmapping");
                        }
                    }
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Delete, ActivityLog.ActivityDetails_Delete2 + "sp_delete_rolemapping");
                }
                nv = null;
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
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

        #endregion

        #region API Acess
        [HttpGet]
        public IActionResult GetAllowedApiByRole(string RoleID)
        {
            DataTable dt = new DataTable();
            string methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("RoleID-INT", RoleID);
                dt = _DAL.GetData("sp_select_roles_byid", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_roles_byid");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_roles_byid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " sp_select_roles_byid" + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }
        #endregion

        #region AdUsers

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult FetchAdUsers()
        {
            DataTable dt = new DataTable();
            DataTable dtUsers = new DataTable();
            try
            {

                dt = _DAL.GetData("sp_select_ldapsettings2", null, _DAL.CSManagementPortalDatabase);
                string Ldap = dt.Rows[0]["LdapPath"].ToString();
                string userid = dt.Rows[0]["LdapUser"].ToString();
                string password = dt.Rows[0]["LdapPassword"].ToString();

                dtUsers = LoadAdUsers(Ldap, userid, password);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_ldapsettings2");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_ldapsettings2");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest(ex.Message);
            }

            return Ok(dtUsers);

        }

        private DataTable LoadAdUsers(string Ldap, string userid, string password)
        {

            DirectoryEntry myLdapConnection = new DirectoryEntry(Ldap, userid, _CommonMethods.DecryptPassword(password));
            DirectorySearcher search = new DirectorySearcher(myLdapConnection) { Filter = ("(objectClass=user)") };
            search.CacheResults = true;

            SearchResultCollection allResults = search.FindAll();
            string varStr = myLdapConnection.Name.ToString();
            string[] varDomain = varStr.Split('=');

            DataTable table = new DataTable();
            table.TableName = "AdUsers";
            table.Columns.Add("UserName");
            table.Columns.Add("UserID");
            table.Columns.Add("EmailID");
            table.Columns.Add("Designation");
            table.Columns.Add("Department");
            table.Columns.Add("TelephoneNo");
            table.Columns.Add("DomainName");
            foreach (SearchResult searchResult in allResults)
            {
                string UserName = "";
                string UserID = "";
                string EmailID = "";
                string Department = "";
                string Designation = "";
                string TelephoneNo = "";
                string DomainName = varDomain[1];
                if (searchResult.Properties["name"] != null && searchResult.Properties["name"].Count > 0)
                {
                    UserName = searchResult.Properties["name"][0].ToString();
                }
                if (UserName == "MettisDev2")
                {

                }
                if (searchResult.Properties["department"] != null && searchResult.Properties["department"].Count > 0)
                {
                    Department = searchResult.Properties["department"][0].ToString();
                }
                if (searchResult.Properties["title"] != null && searchResult.Properties["title"].Count > 0)
                {
                    Designation = searchResult.Properties["title"][0].ToString();
                }
                if (searchResult.Properties["samAccountName"] != null && searchResult.Properties["samAccountName"].Count > 0)
                {
                    UserID = searchResult.Properties["samAccountName"][0].ToString();
                }
                if (searchResult.Properties["mail"] != null && searchResult.Properties["mail"].Count > 0)
                {
                    EmailID = searchResult.Properties["mail"][0].ToString();
                }
                if (searchResult.Properties["telephonenumber"] != null && searchResult.Properties["telephonenumber"].Count > 0)
                {
                    TelephoneNo = searchResult.Properties["telephonenumber"][0].ToString();
                }

                DataRow NewRow = table.NewRow();
                NewRow["UserName"] = UserName; ;
                NewRow["UserID"] = UserID;
                NewRow["EmailID"] = EmailID;
                NewRow["Designation"] = Designation;
                NewRow["Department"] = Department;
                NewRow["TelephoneNo"] = TelephoneNo;
                NewRow["DomainName"] = DomainName;
                table.Rows.Add(NewRow);
            }
            return table;
        }
        #endregion
        
        #region SmtpSetting

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SmtpSettingUpdate([FromBody] SMTPSettings smtpSetting)
        {

            bool Result = false;

            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("ID-INT", smtpSetting.ID.ToString());
                nv.Add("Smtp-VARCHAR", HttpUtility.HtmlEncode(smtpSetting.Smtp));
                nv.Add("SenderEmailID-VARCHAR", HttpUtility.HtmlEncode(smtpSetting.SenderEmailID));
                nv.Add("SmtpPassword-VARCHAR", smtpSetting.SmtpPassword);
                nv.Add("SmtpPort-VARCHAR", HttpUtility.HtmlEncode(smtpSetting.SmtpPort));
                nv.Add("DisplayName-VARCHAR", HttpUtility.HtmlEncode(smtpSetting.DisplayName));
                nv.Add("EnableSSL-BIT", smtpSetting.EnableSSL.ToString());

                Result = _DAL.InsertData("sp_insert_smtpsetting", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_smtpsetting");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_smtpsetting");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest(ex.Message);
            }

            return Ok(Result);


        }

        [HttpGet]
        public IActionResult SmtpSettingGet()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_SmtpSettings", null, _DAL.CSManagementPortalDatabase);
                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_SmtpSettings");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_SmtpSettings");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest(ex.Message);
            }

            return Ok(dt);
        }
        #endregion

        #region UserProfile
        [RateLimitMiddleware(10000, 5)]
        [HttpPost]
        public IActionResult UpdateProfile([FromBody] UserProfile UserProfile)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("UserID-INT", UserProfile.UserID);
                nv.Add("UserName-VARCHAR", HttpUtility.HtmlEncode(UserProfile.UserName));
                nv.Add("DefaultFormID-INT", UserProfile.FormID);
                nv.Add("UserImage-VARCHAR", UserProfile.ImageFile == null ? "NULL" : HttpUtility.HtmlEncode(UserProfile.ImageFile));
                Result = _DAL.InsertData("sp_update_profile", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_update_profile");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_update_profile");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest(ex.Message);
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

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetSmtpGrid()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_selectall_smtpsettings", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_selectall_smtpsettings");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_selectall_smtpsettings");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SetDefaultSMTP([FromBody] SMTPDefaultModel model)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("ID-INT", model.ID.ToString());
                Result = _DAL.InsertData("sp_SetDefaultSMTP", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_SetDefaultSMTP");

                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_SetDefaultSMTP");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
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

        #endregion

        #region Organization Chart

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_allusers", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_allusers");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_allusers");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                BadRequest(ex.Message);
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveOrganizationChart([FromBody] OrganizationChart chart)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("organizationcode-VARCHAR", HttpUtility.HtmlEncode(chart.organizationname));
                nv.Add("organizationname-VARCHAR", HttpUtility.HtmlEncode(chart.organizationname));
                nv.Add("isactive-BIT", chart.isactive == true ? "1" : "0");
                nv.Add("employeeids-VARCHAR", string.Join(",", chart.employeeid));
                nv.Add("managerid-INT", chart.managerid.ToString());
                nv.Add("createdby-INT", chart.createdby.ToString());
                nv.Add("isedit-BIT", chart.isedit.ToString());

                Result = _DAL.InsertData("sp_insert_organizationchart", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_organizationchart");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_organizationchart");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Adminsitrator");
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

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetOrganizationChart()
        {
            DataTable dt = new DataTable();

            try
            {
                dt = _DAL.GetData("sp_select_organizationchart", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_organizationchart");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_organizationchart");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetEmployeeListByManagerID(int managerid)
        {
            DataTable dt = new DataTable();

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("managerid-INT", managerid.ToString());
                dt = _DAL.GetData("sp_select_employeelistbymanagerid", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_organizationchart");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_organizationchart");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your System Adminsitrator");
            }
            return Ok(dt);
        }
        #endregion

    }
}
