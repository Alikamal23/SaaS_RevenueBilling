using RevenueBillingApi.Context;
using RevenueBillingApi.Models.Sessions;
using RevenueBillingApi.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Data;
using System.Reflection;

namespace RevenueBillingApi.Controllers
{
    [Route("api/{controller}/{action}/{id:int?}")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly DataAccessLayer _DAL;
        private readonly ILogger<SessionsController> _logger;
        public SessionsController(DataAccessLayer DAL, ILogger<SessionsController> logger)
        {
            _DAL = DAL;
            _logger = logger;
        }

        #region Methods

        [HttpPost]
        public IActionResult GetErrorInfo()
        {

            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_errorinfo", null, _DAL.CSManagementPortalDatabase);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetErrorInfo", "sp_select_errorinfo", ex.Message);
                return BadRequest(ex.Message);
            }

            _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetErrorInfo", "sp_select_errorinfo", "OK");
            return Ok(dt);
        }

        [HttpPost]
        public IActionResult FillPasswordPolicy()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_userpolicies", null, _DAL.CSManagementPortalDatabase);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "FillPasswordPolicy", "sp_select_userpolicies", ex.Message);
                
                return BadRequest();
            }

            _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "FillPasswordPolicy", "sp_select_userpolicies", "OK");
            return Ok(dt);
        }

        [RateLimitMiddleware(10, 5)]
        [HttpPost]
        public IActionResult InsertUserSession(UserSession session_)
        {

            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();

                nv.Clear();
                nv.Add("UserID-int", session_.UserID);
                nv.Add("SessionID-varchar", session_.SessionID);
                nv.Add("IsLogin-bit", session_.IsLogin);
                Result = _DAL.InsertData("sp_insert_usersession", nv, _DAL.CSManagementPortalDatabase);
                nv = null;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "InsertUserSession", "sp_insert_usersession", ex.Message);
                
                return BadRequest();
            }

            if (Result)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "InsertUserSession", "sp_insert_usersession", "OK");
                return Ok(Result);
            }
            else
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "InsertUserSession", "sp_insert_usersession", "BadRequest");
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(10, 5)]
        [HttpPost]
        public IActionResult CheckLoginCreds(LoginCreds logincreds_)
        {

            bool Result = false;

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("LoginName-VARCHAR", logincreds_.LoginName);
                nv.Add("UserPassword-VARCHAR", logincreds_.UserPassword);
                Result = _DAL.CheckData("sp_select_user", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "CheckLoginCreds", "sp_select_user", ex.Message);
                
                return BadRequest();
            }

            if (Result)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "CheckLoginCreds", "sp_select_user", "OK");
                return Ok(Result);
            }
            else
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "CheckLoginCreds", "sp_select_user", "BadRequest");
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(10, 5)]
        [HttpPost]
        public IActionResult GetUserInfo(LoginCreds logincreds_)
        {

            DataTable Result = new DataTable();

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("LoginName-VARCHAR", logincreds_.LoginName);
                nv.Add("UserPassword-VARCHAR", logincreds_.UserPassword);
                Result = _DAL.GetData("sp_select_user", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserInfo", "sp_select_user", ex.Message);

                
                return BadRequest();
            }

            if (Result != null && Result.Rows.Count > 0)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserInfo", "sp_select_user", "OK");
                return Ok(Result);
            }
            else
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserInfo", "sp_select_user", "BadRequest");
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(10, 5)]
        [HttpPost]
        public IActionResult GetUserInfoAD(LoginCreds logincreds_)
        {

            DataTable Result = new DataTable();

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("LoginName-VARCHAR", logincreds_.LoginName);
                Result = _DAL.GetData("sp_select_user_Ad", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserInfoAD", "sp_select_user_Ad", ex.Message);
                
                return BadRequest();
            }

            if (Result != null && Result.Rows.Count > 0)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserInfoAD", "sp_select_user_Ad", "OK");
                return Ok(Result);
            }
            else
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserInfoAD", "sp_select_user_Ad", "BadRequest");
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(25, 5)]
        [HttpPost]
        public IActionResult GetUserRole(LoginCreds logincreds_)
        {

            DataTable Result = new DataTable();

            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("RoleID-INT", logincreds_.RoleID);

                Result = _DAL.GetData("sp_select_rolesmapping", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "", "", ex.Message);
                
                return BadRequest();
            }

            if (Result != null && Result.Rows.Count > 0)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "", "", "OK");
                return Ok(Result);
            }
            else
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "", "", "BadRequest");
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(10, 5)]
        [HttpPost]
        public bool ResetUserAttemptCount(LoginCreds logincreds_)
        {
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("LoginName-varchar", logincreds_.LoginName);

                bool Result = _DAL.InsertData("sp_update_resetuserattemptcount", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

                if (Result)
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "ResetUserAttemptCount", "sp_update_resetuserattemptcount", "OK");
                    return Result;
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "ResetUserAttemptCount", "sp_update_resetuserattemptcount", "BadRequest");
                    return Result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "ResetUserAttemptCount", "sp_update_resetuserattemptcount", ex.Message);
                return false;
            }
        }

        [RateLimitMiddleware(10, 5)]
        [HttpPost]
        public string? UpdateUserAttemptCount(LoginCreds logincreds_)
        {
            DataTable dataTable = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("LoginName-varchar", logincreds_.LoginName);

                dataTable = _DAL.GetData("sp_update_userattemptcount", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "UpdateUserAttemptCount", "sp_update_userattemptcount", "OK");
                    return dataTable.Rows[0][0].ToString();
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "UpdateUserAttemptCount", "sp_update_userattemptcount", "BadRequest");
                    return "No Data Found";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "UpdateUserAttemptCount", "sp_update_userattemptcount", ex.Message);
                
                return "Something Went Wrong Please Contact Your Sysmtem Adminsitrator";
            }
        }

        [RateLimitMiddleware(3, 1440)]
        [HttpPost]
        public string? ChangePassword_Login(LoginCreds logincreds_)
        {
            DataTable dataTable = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("UserID-INT", logincreds_.UserID);
                nv.Add("UserPasswordOld-VARCHAR", logincreds_.UserPassword_Old);
                nv.Add("UserPasswordNew-VARCHAR", logincreds_.UserPassword_New);

                dataTable = _DAL.GetData("sp_update_password", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "ChangePassword_Login", "sp_update_password", "OK");
                    return dataTable.Rows[0][0].ToString();
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "ChangePassword_Login", "sp_update_password", "BadRequest");
                    return "No Data Found";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "ChangePassword_Login", "sp_update_password", ex.Message);
                
                return "Something Went Wrong Please Contact Your Sysmtem Adminsitrator";
            }
        }

        [RateLimitMiddleware(20, 5)]
        [HttpPost]
        public string CheckOldPassword(LoginCreds logincreds_)
        {
            DataTable dataTable = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("UserID-INT", logincreds_.UserID);
                nv.Add("UserPasswordOld-VARCHAR", logincreds_.UserPassword);

                dataTable = _DAL.GetData("sp_select_checkoldpassword", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "CheckOldPassword", "sp_select_checkoldpassword", "OK");
                    return dataTable.Rows[0][0].ToString();
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "CheckOldPassword", "sp_select_checkoldpassword", "BadRequest");
                    return "No Data Found";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "CheckOldPassword", "sp_select_checkoldpassword", ex.Message);
                
                return "Something Went Wrong Please Contact Your Sysmtem Adminsitrator";
            }
        }

        [RateLimitMiddleware(20, 5)]
        [HttpPost]
        public IActionResult CheckUserPassword(LoginCreds logincreds_)
        {
            DataTable dataTable = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("UserID-INT", logincreds_.UserID);
                dataTable = _DAL.GetData("sp_select_userpasswordhistory", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "CheckUserPassword", "sp_select_userpasswordhistory", "OK");
                    return Ok(dataTable);
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "CheckUserPassword", "sp_select_userpasswordhistory", "BadRequest");
                    return BadRequest("No Data Found");
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "CheckUserPassword", "sp_select_userpasswordhistory", ex.Message);
                
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
        }

        [RateLimitMiddleware(20, 5)]
        [HttpPost]
        public string? GetUserHashPassword(LoginCreds logincreds_)
        {
            DataTable dataTable = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("LoginName-VARCHAR", logincreds_.LoginName);
                dataTable = _DAL.GetData("sp_select_userhashpassword", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserHashPassword", "sp_select_userhashpassword", "OK");
                    return dataTable.Rows[0][0].ToString();
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserHashPassword", "sp_select_userhashpassword", "BadRequest");
                    return "No Data Found";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserHashPassword", "sp_select_userhashpassword", ex.Message);
                
                return "Something Went Wrong Please Contact Your Sysmtem Adminsitrator";
            }
        }

        [RateLimitMiddleware(20, 5)]
        [HttpPost]
        public IActionResult GetUserEmailVerification(EmailVerification emailVerification_)
        {
            DataTable dataTable = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("UserEmail-VARCHAR", emailVerification_.UserEmail);
                dataTable = _DAL.GetData("sp_select_useremailverification", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserEmailVerification", "sp_select_useremailverification", "OK");
                    return Ok(dataTable);
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserEmailVerification", "sp_select_useremailverification", "BadRequest");
                    return BadRequest("No Data Found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetUserEmailVerification", "sp_select_useremailverification", ex.Message);
                
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
        }

        [RateLimitMiddleware(20, 5)]
        [HttpPost]
        public IActionResult ResetUserPassword(EmailVerification emailVerification_)
        {
            bool Result = false;
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("UserEmail-VARCHAR", emailVerification_.UserEmail);
                nv.Add("LoginName-VARCHAR", emailVerification_.LoginName);
                nv.Add("UserPassword-VARCHAR", emailVerification_.UserPassword);
                Result = _DAL.InsertData("sp_update_resetuserpassword", nv, _DAL.CSManagementPortalDatabase);

                nv = null;

                if (Result)
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "ResetUserPassword", "sp_update_resetuserpassword", "OK");
                    return Ok(Result);
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "ResetUserPassword", "sp_update_resetuserpassword", "BadRequest");
                    return BadRequest(Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "ResetUserPassword", "sp_update_resetuserpassword", ex.Message);
                
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
        }

        [RateLimitMiddleware(20, 5)]
        [HttpPost]
        public IActionResult GetSMTPSettings()
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("WithPassword-INT", "1");
                dt = _DAL.GetData("sp_select_SmtpSettings", nv, _DAL.CSManagementPortalDatabase);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetSMTPSettings", "sp_select_SmtpSettings", ex.Message);
                
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetSMTPSettings", "sp_select_SmtpSettings", "OK");
            return Ok(dt);

        }

        [RateLimitMiddleware(20, 5)]
        [HttpPost]
        public IActionResult GetNavigationSettings(LoginCreds logincreds_)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("RoleID-INT", logincreds_.RoleID);
                dt = _DAL.GetData("sp_select_navigation", nv, _DAL.CSManagementPortalDatabase);
                nv = null;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetNavigationSettings", "sp_select_navigation", ex.Message);
                
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "GetNavigationSettings", "sp_select_navigation", "OK");
            return Ok(dt);

        }

        [HttpPost]
        public bool SystemActivityLog(ActivityLog activityLog_)
        {
            bool Result = false;

            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("FormID-INT", activityLog_.FormID);
                nv.Add("ActivityID-INT", activityLog_.ActivityID);
                nv.Add("UserID-INT", activityLog_.UserID);
                nv.Add("ActivityDetails-VARCHAR", activityLog_.ActivityDetails);

                Result = _DAL.InsertData("sp_insert_activitylog", nv, _DAL.CSManagementPortalDatabase);

                nv = null;
                if (Result)
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "SystemActivityLog", "sp_insert_activitylog", "OK");
                    return Result;
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "SystemActivityLog", "sp_insert_activitylog", "BadRequest");
                    return Result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "SystemActivityLog", "sp_insert_activitylog", ex.Message);
                return Result;
            }
        }
        [RateLimitMiddleware(10000, 5)]
        [HttpGet]
        public IActionResult FillLAllowedFilesExt()
        {
            DataTable dt = new DataTable();
            try
            {

                dt = _DAL.GetData("sp_select_allowedfileextension", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "FillLAllowedFilesExt", "sp_select_allowedfileextension", "OK");
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "FillLAllowedFilesExt", "sp_select_allowedfileextension", "No Data Found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("{0} {1} {2} {3}", "SessionsController", "FillLAllowedFilesExt", "sp_select_allowedfileextension", ex.Message);
                return BadRequest(ex.Message);
            }

            return Ok(dt);

        }
        #endregion

    }
}
