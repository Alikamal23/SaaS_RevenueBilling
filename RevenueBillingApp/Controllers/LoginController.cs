using RevenueBillingApp.Extensions;
using RevenueBillingApp.Models.Authentication;
using RevenueBillingApp.Models.Session;
using RevenueBillingApp.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.DirectoryServices.AccountManagement;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace RevenueBillingApp.Controllers
{
    [Authorize(AuthenticationSchemes = "ASPXAUTH")]
    public class LoginController : Controller
    {
        public static NameValueCollection nv = new NameValueCollection();
        private readonly Sessions _sessions;
        private readonly IConfiguration _config;
        private readonly ILogger<LoginController> _logger;
        private readonly SendEmail _sendemail;
        private readonly DataEncryptor _dataencryptor;
        private readonly RandomStringGenerator _randomstringgenerator;
        private readonly CommonMethods _commonmethods;
        public LoginController(Sessions sessions, IConfiguration config, ILogger<LoginController> logger, SendEmail sendemail, DataEncryptor dataencryptor, RandomStringGenerator randomstringgenerator, CommonMethods commonmethods)
        {
            _sessions = sessions;
            _config = config;
            _logger = logger;
            _sendemail = sendemail;
            _dataencryptor = dataencryptor;
            _randomstringgenerator = randomstringgenerator;
            _commonmethods = commonmethods;
        }

        [AllowAnonymous]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ActionName("ForgetPassword")]
        public IActionResult ForgetUserPassword(ForgetPassword forgetpassword)
        {

            _logger.LogInformation("{0} Email:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, forgetpassword.UserEmail, ActivityLog.Details_View, "Forget Password");

            string email = forgetpassword.UserEmail;
            VerifyEmail verifyemail = _sessions.GetUserEmailVerification(forgetpassword.UserEmail);
            string hashpassword = string.Empty;
            if (verifyemail.IsVerified != null && verifyemail.IsVerified == true)
            {
                DateTime futureDate = DateTime.Now.AddDays(7);
                string formattedFutureDate = futureDate.ToString("yyyy-MM-dd");

                if (verifyemail.PasswordCreateDate == null || verifyemail.PasswordCreateDate > futureDate)
                {
                    string RandomStrings = _randomstringgenerator.GetRandomString();
                    List<SMTPSettings> SMTPSettings = _sessions.GetSMTPSettings();

                    string EmailBody = ActivityLog.EmailBody + " RandomPassword : " + RandomStrings;

                    bool SendMail = _sendemail.SendEmailToUsers(new List<string> { forgetpassword.UserEmail }, EmailBody, ActivityLog.EmailSubject, SMTPSettings);

                    if (SendMail)
                    {
                        _logger.LogInformation("{0} Email:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, forgetpassword.UserEmail, "Email Sent", "Forget Password");

                        hashpassword = _dataencryptor.HashPassword(RandomStrings);

                        bool resetpassword = _sessions.ResetUserPassword(verifyemail.LoginName, forgetpassword.UserEmail, hashpassword);

                        if (resetpassword)
                        {
                            ViewBag.Message = "Your Password Has Been Reset";
                            _logger.LogInformation("{0} Email:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, forgetpassword.UserEmail, "Password Has Been Reset", "Forget Password");

                            return RedirectToAction("UserLogin");
                        }
                        else
                        {
                            ViewBag.Message = "Please Try Again Or Contact System Administrator";
                            _logger.LogInformation("{0} Email:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, forgetpassword.UserEmail, "Password Not Reset", "Forget Password");
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Please Try Again Or Contact System Administrator";
                        _logger.LogInformation("{0} Email:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, forgetpassword.UserEmail, "Email Not Sent", "Forget Password");

                        return View();
                    }
                }
                else
                {
                    ViewBag.Message = "You Already Requested For Reset Password. Please Contact System Administrator";
                    _logger.LogInformation("{0} Email:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, forgetpassword.UserEmail, "You Already Requested For Reset Password. Please Contact System Administrator", "Forget Password");

                    return View();
                }
            }
            else
            {
                ViewBag.Message = "Your Request Has Been Sent On Provided Email.";
                _logger.LogInformation("{0} Email:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, forgetpassword.UserEmail, "User Has Provided Different Email", "Forget Password");

                return View();
            }
        }

        [AllowAnonymous]
        public IActionResult ChangePassword(ChangePassword changePassword)
        {

            _logger.LogInformation("{0} {1} {2}", "Login/"+ MethodBase.GetCurrentMethod().Name, ActivityLog.Details_View, "Change Password");

            return View(changePassword);
        }

        [AllowAnonymous]
        [HttpPost]
        [ActionName("ChangePassword")]
        public IActionResult ChangeUserPassword(ChangePassword changePassword)
        {
            string? UserID = HttpContext.Session.GetString("UserID");
            string? UserPassword = HttpContext.Session.GetString("UserPassword");
            bool VerifyPassword = _commonmethods.VerifyPassword(changePassword.oldpassword, UserPassword);

            _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, ActivityLog.Details_View, "Change Password");
            if (VerifyPassword)
            {

                if (changePassword.newpassword != changePassword.oldpassword && changePassword.newpassword == changePassword.confirmpassword)
                {
                    List<PasswordHistory> ChkPassResult = _sessions.CheckUserPassword(UserID);
                    int VerifyOldPassword = ChkPassResult.Where(x => _commonmethods.VerifyPassword(changePassword.confirmpassword, x.UserPassword)).Count();

                    if (VerifyOldPassword == 0)
                    {

                        if (_sessions.UserPolicies.AlphaType == true)
                        {
                            string pattern = @"[a-zA-Z]";
                            Match match = Regex.Match(changePassword.confirmpassword, pattern);
                            int count = Regex.Matches(changePassword.confirmpassword, pattern).Count;

                            if (match.Success)
                            {
                                if (count < _sessions.UserPolicies.MinAlphaChar)
                                {
                                    ViewBag.MinAlphaChar = "Enter Atleast " + _sessions.UserPolicies.MinAlphaChar + " Characters.";
                                    _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, "UserPolicies.MinAlphaChar", "Change Password");
                                    return View(changePassword);
                                }
                            }
                            else
                            {
                                ViewBag.AlphaType = "You Must Enter Characters From a-z And A-Z";
                                _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, "UserPolicies.MinAlphaChar", "Change Password");
                                return View(changePassword);
                            }
                        }
                        if (_sessions.UserPolicies.CharCaps == true)
                        {
                            string pattern = "[A-Z]";

                            int count = Regex.Matches(changePassword.confirmpassword, pattern).Count;

                            if (count < _sessions.UserPolicies.MinCharCaps)
                            {
                                ViewBag.MinCharCaps = "Enter Atleast " + _sessions.UserPolicies.MinCharCaps + " Caps Characters.";
                                _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, "UserPolicies.MinCharCaps", "Change Password");
                                return View(changePassword);
                            }
                        }
                        if (_sessions.UserPolicies.NumericType == true)
                        {
                            string pattern = @"\d";
                            int count = Regex.Matches(changePassword.confirmpassword, pattern).Count;

                            if (count < _sessions.UserPolicies.MinNumericChar)
                            {
                                ViewBag.MinNumericChar = "Enter Atleast " + _sessions.UserPolicies.MinNumericChar + " Numeric Characters.";
                                _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, "UserPolicies.MinNumericChar", "Change Password");
                                return View(changePassword);
                            }
                        }
                        if (_sessions.UserPolicies.IsSpecialChar == true)
                        {
                            string pattern = @"[@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]";
                            int count = Regex.Matches(changePassword.confirmpassword, pattern).Count;

                            if (count < _sessions.UserPolicies.MinSpecialChar)
                            {
                                ViewBag.MinSpecialChar = "Enter Atleast " + _sessions.UserPolicies.MinSpecialChar + " Special Characters.";
                                _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, "UserPolicies.MinSpecialChar", "Change Password");
                                return View(changePassword);
                            }

                        }

                        string ChgPassResult = _sessions.ChangePassword_Login(UserID, _commonmethods.HashPassword(changePassword.confirmpassword), UserPassword);

                        if (ChgPassResult == "Password Updated Successfully!")
                        {
                            _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, ChgPassResult, "Change Password");

                            ViewBag.ChgPassResult = "Password Updated Successfully!";
                            HttpContext.Session.Clear();
                            Response.Cookies.Delete(".AspNetCore.Session");
                            return RedirectToAction("UserLogin");
                        }
                        else
                        {
                            _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, ChgPassResult, "Change Password");
                            return View(changePassword);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, "Using Old Password As New Password", "Change Password");

                        ViewBag.ChkPassResult = "Something Went Wrong! Please Contact System Administrator";
                        return View(changePassword);
                    }
                }
                else
                {
                    ViewBag.PasswordMatch = "Something Went Wrong! Please Contact System Administrator";
                    _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, "Password Dont Match", "Change Password");
                    return View(changePassword);
                }
            }
            else
            {
                ViewBag.OldPasswordMatch = "Something Went Wrong! Please Contact System Administrator";
                _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, "Incorrect Old Password", "Change Password");

                return View(changePassword);
            }

        }

        [AllowAnonymous]
        public IActionResult UserLogin()
        {
            ClaimsPrincipal claimsPrincipal = HttpContext.User;
            var UniqueKey = (from c in claimsPrincipal.Claims where c.Type == "Guid" select c.Value).FirstOrDefault();
            var UserID = (from c in claimsPrincipal.Claims where c.Type == "UserID" select c.Value).FirstOrDefault();
            ViewBag.ErrorMessage = null;
            if (_sessions.SessionExist(UniqueKey, UserID))
            {
                try
                {

                    _logger.LogInformation("{0} UserID:{1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, UserID, ActivityLog.Details_View, "UserLogin");

                    SessionItems session = _sessions.GetSession(UniqueKey, UserID);
                    return Redirect(session.userInfo.DefaultFormName);
                }
                catch(Exception ex)
                {
                    _logger.LogCritical("Login/" + MethodBase.GetCurrentMethod().Name + ex.Message);
                }
            }
            return View();
        }


        //public async Task<IActionResult> UserLogin(LoginInfo loginInfo)
        //{
        //    string Message = "";

        //    Tokens token = new Tokens();

        //    List<UserInfo> userInfos = new List<UserInfo>();

        //    _logger.LogInformation("{0} {1} {2}", "Login/"+ MethodBase.GetCurrentMethod().Name, ActivityLog.Details_View, "UserLogin");

        //    try
        //    {
        //        if (loginInfo != null)
        //        {
        //            string? Hash_password = _sessions.GetUserHashPassword(loginInfo.LoginName);
        //            bool VerifyUser = false;
        //            if (Hash_password != null)
        //            {
        //                VerifyUser = _dataencryptor.VerifyPassword(loginInfo.Password, Hash_password);
        //            }

        //            if (VerifyUser)
        //            {
        //                bool IsLoginCreds = _sessions.CheckLoginCreds(loginInfo.LoginName, Hash_password);

        //                if (IsLoginCreds)
        //                {
        //                    userInfos = _sessions.GetUserInfo(loginInfo.LoginName, Hash_password);

        //                    if (userInfos.Count > 0)
        //                    {
        //                        bool CheckBlockTime = ((userInfos[0].BlockTime == null) ? true : false);
        //                        DateTime startTime = Convert.ToDateTime(userInfos[0].BlockTime);
        //                        DateTime endTime = DateTime.Now;

        //                        TimeSpan span = endTime.Subtract(startTime);
        //                        var usertime = (int)Math.Round(span.TotalMinutes);

        //                        if (CheckBlockTime || _sessions.UserPolicies.IntervalBadeLoginMinuts < usertime)
        //                        {
        //                            List<RolesMapping> rolesMappings = _sessions.GetUserRole(userInfos[0].RoleID.ToString());
        //                            List<Navigation> navigations = _sessions.GetNavigationSettings(userInfos[0].RoleID);
        //                            List<Form> _Form = _commonmethods.CreateObject(navigations);

        //                            token = _sessions.GetApiToken(loginInfo.LoginName, Hash_password);

        //                            string encrypteduniquekey = "";

        //                            AuthenticationProperties authenticationProperty = new AuthenticationProperties();

        //                            Guid g = Guid.NewGuid();
        //                            string uniquekey = "UserID=" + userInfos[0].UserID.ToString() + "&GUID=" + g;
        //                            encrypteduniquekey = _dataencryptor.EncryptPassword(uniquekey);

        //                            if (loginInfo.RememberMe != null && loginInfo.RememberMe == "on")
        //                            {
        //                                authenticationProperty.IsPersistent = true;
        //                                authenticationProperty.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(365);
        //                            }
        //                            else
        //                            {
        //                                authenticationProperty.IsPersistent = false;
        //                            }

        //                            if (userInfos[0].IsReset == true || userInfos[0].RemainingExpiryDays <= 0)
        //                            {
        //                                HttpContext.Session.SetString("UserID", userInfos[0].UserID.ToString());
        //                                HttpContext.Session.SetString("UserPassword", Hash_password);
        //                                _logger.LogInformation("{0} {1} {2}", "Login/"+ MethodBase.GetCurrentMethod().Name, "User Have To Change Password Based On Policy", "UserLogin");
        //                                return RedirectToAction("ChangePassword");
        //                            }
        //                            else
        //                            {
        //                                var claims = new List<Claim>
        //                                {
        //                                    new Claim("UserID",userInfos[0].UserID.ToString()),
        //                                    new Claim("Guid",encrypteduniquekey),
        //                                    new Claim(ClaimTypes.Name,userInfos[0].UserName),
        //                                    new Claim(ClaimTypes.Email,userInfos[0].UserEmail),
        //                                    new Claim(ClaimTypes.Role,userInfos[0].RoleID.ToString())
        //                                };

        //                                var claimIdentity = new ClaimsIdentity(claims, "IdentityClaim");
        //                                var claimPrinciple = new ClaimsPrincipal(claimIdentity);

        //                                await HttpContext.SignInAsync("ASPXAUTH", claimPrinciple, authenticationProperty);
        //                                bool IsSession = _sessions.AddSession(userInfos[0], rolesMappings, token, encrypteduniquekey, _Form, _sessions.UserPolicies.AllowConcurrentLogin);

        //                                if (IsSession)
        //                                {
        //                                    userInfos[0].SessionID = encrypteduniquekey;
        //                                    string formname = "/" + userInfos[0].DefaultFormName;
        //                                    _sessions.ResetUserAttemptCount(loginInfo.LoginName);
        //                                    _sessions.SystemActivityLog(0, ActivityLog.ID_Login, userInfos[0].UserID, ActivityLog.Details_Login);
        //                                    _logger.LogInformation("{0} {1} {2} {3}", "Login/"+ MethodBase.GetCurrentMethod().Name, userInfos[0].UserID, "User Logged In From Login To " + userInfos[0].DefaultFormName, "UserLogin");
        //                                    return Redirect(formname);
        //                                }
        //                                else
        //                                {
        //                                    Message = "Session Not Started!";
        //                                    ViewBag.IsError = true;
        //                                    ViewBag.ErrorMessage = Message;
        //                                    _logger.LogInformation("{0} {1} {2}", "Login/"+ MethodBase.GetCurrentMethod().Name, "Session Not Started!", "UserLogin");

        //                                    return View();
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            int RemainingTime = _sessions.UserPolicies.IntervalBadeLoginMinuts - usertime;
        //                            Message = "Your Account Has Been Temporarily Blocked For " + _sessions.UserPolicies.IntervalBadeLoginMinuts + " Mins.Please Try Again After " + RemainingTime + " Mins.";
        //                            ViewBag.IsError = true;
        //                            ViewBag.ErrorMessage = Message;
        //                            _logger.LogInformation("{0} {1} {2}", "Login/"+ MethodBase.GetCurrentMethod().Name, "User Account Has Been Blocked", "UserLogin");
        //                            return View();

        //                        }
        //                    }
        //                    else
        //                    {
        //                        ViewBag.IsError = true;
        //                        ViewBag.ErrorMessage = "Your Account Has Been Blocked. Please Contact Your System Administrator.";
        //                        _logger.LogInformation("{0} {1} {2}", "Login/" + MethodBase.GetCurrentMethod().Name, "User Account Has Been Blocked", "UserLogin");

        //                        return View();
        //                    }

        //                }
        //                else
        //                {
        //                    _sessions.UpdateUserAttemptCount(loginInfo.LoginName);

        //                    ViewBag.IsError = true;
        //                    ViewBag.ErrorMessage = "Login Name Or Password Incorrect!";
        //                    _logger.LogInformation("{0} {1} {2}", "Login/" + MethodBase.GetCurrentMethod().Name, "Login Name Or Password Incorrect!", "UserLogin");

        //                    return View();
        //                }
        //            }
        //            else
        //            {

        //                Message = "Login Name Or Password Incorrect!";
        //                _logger.LogInformation("{0} {1} {2}", "Login/" + MethodBase.GetCurrentMethod().Name, "Login Name Or Password Incorrect!", "UserLogin");
        //                ViewBag.ErrorMessage = Message;
        //                return View();
        //            }
        //        }
        //        else
        //        {
        //            Message = "Model State Invalid!";
        //            ViewBag.ErrorMessage = Message;
        //            _logger.LogInformation("{0} {1} {2}", "Login/" + MethodBase.GetCurrentMethod().Name, "Model State Invalid", "UserLogin");
        //            return View();
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Message = ex.Message;
        //        _logger.LogCritical("{0} {1}", "Login/"+ MethodBase.GetCurrentMethod().Name, ex.Message);

        //        ViewBag.ErrorMessage = Message;
        //    }

        //    return View();
        //}

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> UserLogin(LoginInfo loginInfo)
        {

            string Message = "";

            Tokens token = new Tokens();

            List<UserInfo> userInfos = new List<UserInfo>();

            _logger.LogInformation("{0} {2} {3}", "Login/Index", ActivityLog.Details_View, "Index");

            try
            {
                if (loginInfo != null)
                {
                    string? Hash_password = _sessions.GetUserHashPassword(loginInfo.LoginName);
                    bool VerifyUser = false;
                    if (Hash_password != null)
                    {
                        VerifyUser = _commonmethods.VerifyPassword(loginInfo.Password, Hash_password);
                    }

                    bool IsLoginCreds = _sessions.CheckLoginCreds(loginInfo.LoginName, Hash_password);

                    if (VerifyUser && IsLoginCreds)
                    {
                        userInfos = _sessions.GetUserInfo(loginInfo.LoginName, Hash_password);

                        if (userInfos.Count > 0)
                        {
                            bool CheckBlockTime = ((userInfos[0].BlockTime == null) ? true : false);
                            DateTime startTime = Convert.ToDateTime(userInfos[0].BlockTime);
                            DateTime endTime = DateTime.Now;
                            TimeSpan span = endTime.Subtract(startTime);
                            var usertime = (int)Math.Round(span.TotalMinutes);

                            var inactiveindays = _sessions.UserPolicies.inactiveindays;
                            var Sessiontime = (userInfos[0].SessionDateTime == null) ? DateTime.Now : userInfos[0].SessionDateTime;
                            DateTime sessionTime = Convert.ToDateTime((userInfos[0].SessionDateTime == null) ? DateTime.Now : userInfos[0].SessionDateTime);
                            //DateTime sessionTime = Convert.ToDateTime(userInfos[0].SessionDateTime);
                            DateTime currentTime = DateTime.Now;
                            TimeSpan sessionspan = currentTime.Subtract(sessionTime);
                            int TotalDays = (int)Math.Round(sessionspan.TotalDays);

                            if (TotalDays <= _sessions.UserPolicies.inactiveindays || inactiveindays == 0)
                            {
                                if (CheckBlockTime || _sessions.UserPolicies.IntervalBadeLoginMinuts < usertime)
                                {
                                    List<RolesMapping> rolesMappings = _sessions.GetUserRole(userInfos[0].RoleID.ToString());
                                    List<Navigation> navigations = _sessions.GetNavigationSettings(userInfos[0].RoleID);
                                    List<Form> _Form = _commonmethods.CreateObject(navigations);
                                    token = _sessions.GetApiToken(loginInfo.LoginName, Hash_password);

                                    string encrypteduniquekey = "";

                                    AuthenticationProperties authenticationProperty = new AuthenticationProperties();

                                    Guid g = Guid.NewGuid();
                                    string uniquekey = "UserID=" + userInfos[0].UserID.ToString() + "&GUID=" + g;
                                    encrypteduniquekey = _commonmethods.EncryptPassword(uniquekey);

                                    if (loginInfo.RememberMe != null && loginInfo.RememberMe == "on")
                                    {
                                        authenticationProperty.IsPersistent = true;
                                        authenticationProperty.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(365);
                                    }
                                    else
                                    {
                                        authenticationProperty.IsPersistent = false;
                                    }

                                    if (userInfos[0].IsReset == true || userInfos[0].RemainingExpiryDays <= 0)
                                    {
                                        HttpContext.Session.SetString("UserID", userInfos[0].UserID.ToString());
                                        HttpContext.Session.SetString("UserPassword", Hash_password);
                                        _logger.LogInformation("{0} {2} {3}", "Login/Index", "User Have To Change Password Based On Policy", "Index");
                                        return RedirectToAction("ChangePassword");
                                    }
                                    else
                                    {
                                        var claims = new List<Claim>
                                        {
                                            new Claim("UserID",userInfos[0].UserID.ToString()),
                                            new Claim("Guid",encrypteduniquekey),
                                            new Claim(ClaimTypes.Name,userInfos[0].UserName),
                                            new Claim(ClaimTypes.Email,userInfos[0].UserEmail),
                                            new Claim(ClaimTypes.Role,userInfos[0].RoleID.ToString())
                                        };

                                        var claimIdentity = new ClaimsIdentity(claims, "IdentityClaim");
                                        var claimPrinciple = new ClaimsPrincipal(claimIdentity);

                                        await HttpContext.SignInAsync("ASPXAUTH", claimPrinciple, authenticationProperty);
                                        bool IsSession = _sessions.AddSession(userInfos[0], rolesMappings, token, encrypteduniquekey, _Form, _sessions.UserPolicies.AllowConcurrentLogin);

                                        if (IsSession)
                                        {
                                            userInfos[0].SessionID = encrypteduniquekey;
                                            string formname = "/" + userInfos[0].DefaultFormName;
                                            _sessions.ResetUserAttemptCount(loginInfo.LoginName);
                                            _sessions.SystemActivityLog(0, ActivityLog.ID_Login, userInfos[0].UserID, ActivityLog.Details_Login);
                                            _logger.LogInformation("{0} {1} {2} {3}", "Login/Index", userInfos[0].UserID, "User Logged In From Login To " + userInfos[0].DefaultFormName, "Index");
                                            return Redirect(formname);
                                        }
                                        else
                                        {
                                            Message = "Session Not Started!";
                                            ViewBag.IsError = true;
                                            ViewBag.ErrorMessage = Message;
                                            _logger.LogInformation("{0} {2} {3}", "Login/Index", "Session Not Started!", "Index");

                                            return View();
                                        }
                                    }
                                }
                                else
                                {
                                    int RemainingTime = _sessions.UserPolicies.IntervalBadeLoginMinuts - usertime;
                                    Message = "Your Account Has Been Temporarily Blocked For " + _sessions.UserPolicies.IntervalBadeLoginMinuts + " Mins.Please Try Again After " + RemainingTime + " Mins.";
                                    ViewBag.IsError = true;
                                    ViewBag.ErrorMessage = Message;
                                    _logger.LogInformation("{0} {2} {3}", "Login/Index", "User Account Has Been Blocked", "Index");
                                    return View();

                                }
                            }

                            else
                            {
                                _sessions.UpdateUserAttemptCount(loginInfo.LoginName);
                                Message = "Your Account Has Been Blocked. Please Contact Your System Administrator.";
                                ViewBag.IsError = true;
                                ViewBag.ErrorMessage = Message;
                                _logger.LogInformation("{0} {2} {3}", "Login/Index", "User Account Has Been Blocked", "Index");
                                return View();

                            }
                        }

                        else
                        {
                            ViewBag.IsError = true;
                            ViewBag.ErrorMessage = "Your Account Has Been Blocked. Please Contact Your System Administrator.";
                            _logger.LogInformation("{0} {2} {3}", "Login/Index", "User Account Has Been Blocked", "Index");

                            return View();
                        }

                    }
                    else
                    {
                        var spmessage = _sessions.UpdateUserAttemptCount(loginInfo.LoginName);
                        ViewBag.IsError = true;
                        ViewBag.ErrorMessage = string.IsNullOrEmpty(spmessage) ? "Login Name or Password Incorrect!" : spmessage;
                        _logger.LogInformation("{0} {2} {3}", "Login/Index", "Login Name Or Password Incorrect!", "Index");

                        return View();
                    }
                }
                else
                {
                    Message = "Model State Invalid!";
                    ViewBag.ErrorMessage = Message;
                    _logger.LogInformation("{0} {2} {3}", "Login/Index", "Model State Invalid", "Index");
                    return View();
                }

            }
            catch (Exception ex)
            {
                Message = ex.Message;
                _logger.LogCritical("{0} {1}", "Login/Index", ex.Message);

                ViewBag.ErrorMessage = Message;
            }

            return View();
        }




        public async Task<IActionResult> Logout()
        {

            try
            {
                try
                {

                    _logger.LogInformation("{0} {1} {2}", "Login/"+ MethodBase.GetCurrentMethod().Name, "User Initiated Logout", "UserLogin");
                    ClaimsPrincipal claimsPrincipal = HttpContext.User;
                    var UniqueKey = (from c in claimsPrincipal.Claims where c.Type == "Guid" select c.Value).FirstOrDefault();
                    var UserID = (from c in claimsPrincipal.Claims where c.Type == "UserID" select c.Value).FirstOrDefault();
                    SessionItems sessionItems = _sessions.GetSession(UniqueKey, UserID);

                    if (sessionItems != null)
                    {
                        if (sessionItems.userInfo != null)
                        {
                            _sessions.RemoveSession(UniqueKey, sessionItems.userInfo.UserID.ToString());
                            _sessions.SystemActivityLog(0, ActivityLog.ID_LogOut, sessionItems.userInfo.UserID, ActivityLog.Details_LogOut);
                        }
                    }

                    await HttpContext.SignOutAsync("ASPXAUTH");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical("LogOut : " + ex.Message);
                }

                HttpContext.Session.Clear();
                _logger.LogInformation("{0} {1} {2}", "Login/"+ MethodBase.GetCurrentMethod().Name, "User Logout Successfully", "UserLogin");
            }
            catch (Exception ex)
            {
                _logger.LogCritical("LogOut : " + ex.Message);
            }
            return RedirectToAction("UserLogin", "Login");
        }

        private bool VerifyDomainUserExist()
        {
            string sDomainName = Environment.UserDomainName;
            string sUserName = Environment.UserName.ToLower();

            bool OK = false;
            _logger.LogInformation("{0} {1}", "VerifyDomainUserExist", "VerifyDomainUserExist");
            using (var domainContext = new PrincipalContext(ContextType.Domain, sDomainName))
            {
                using (var foundUser = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, sUserName))
                {
                    OK = foundUser != null;
                }
            }
            return OK;
        }

        [AllowAnonymous]
        [HttpPost]
        //public async Task<IActionResult> AdLogin()
        //{
        //    string ErrorMessage = "";
        //    bool result = false;
        //    try
        //    {

        //        string domainName = Environment.UserDomainName;
        //        string domainUserName = Environment.UserName;

        //        LoginInfo loginInfo = new LoginInfo();

        //        loginInfo.LoginName = domainUserName;
        //        loginInfo.Password = "NULL";

        //        List<UserInfo> userInfos = _sessions.GetUserInfoAD(loginInfo.LoginName);


        //        if (userInfos.Count > 0)
        //        {
        //            if (userInfos[0].IsAdUser)
        //            {
        //                loginInfo.Email = userInfos[0].UserEmail;

        //                if (VerifyDomainUserExist())
        //                {
        //                    List<RolesMapping> rolesMappings = _sessions.GetUserRole(userInfos[0].RoleID.ToString());

        //                    List<Navigation> navigations = _sessions.GetNavigationSettings(userInfos[0].RoleID);
        //                    List<Form> _Form =  _commonmethods.CreateObject(navigations);
        //                    Tokens tokens = _sessions.GetApiToken(loginInfo.LoginName, loginInfo.Password);

        //                    string encrypteduniquekey = "";

        //                    AuthenticationProperties authenticationProperty = new AuthenticationProperties();

        //                    Guid g = Guid.NewGuid();
        //                    string uniquekey = "UserID=" + userInfos[0].UserID.ToString() + "&GUID=" + g;
        //                    encrypteduniquekey = _dataencryptor.EncryptPassword(uniquekey);

        //                    if (_sessions.UserPolicies.AllowConcurrentLogin)
        //                    {
        //                        authenticationProperty.IsPersistent = true;
        //                    }
        //                    else
        //                    {
        //                        if (loginInfo.RememberMe != null)
        //                        {
        //                            authenticationProperty.IsPersistent = true;
        //                            _logger.LogInformation("{0} {1} {2}", "Login/AdLogin", "IsPersistent = true", "AdLogin");

        //                        }
        //                        else
        //                        {
        //                            authenticationProperty.IsPersistent = false;
        //                            _logger.LogInformation("{0} {1} {2}", "Login/AdLogin", "IsPersistent = False", "AdLogin");

        //                        }

        //                    }

        //                    var claims = new List<Claim>
        //                    {
        //                        new Claim("UserID",userInfos[0].UserID.ToString()),
        //                        new Claim("Guid",encrypteduniquekey),
        //                        new Claim(ClaimTypes.Name,userInfos[0].UserName),
        //                        new Claim(ClaimTypes.Email,userInfos[0].UserEmail),
        //                        new Claim(ClaimTypes.Role,userInfos[0].RoleID.ToString())
        //                    };

        //                    var claimIdentity = new ClaimsIdentity(claims, "IdentityClaim");
        //                    var claimPrinciple = new ClaimsPrincipal(claimIdentity);

        //                    await HttpContext.SignInAsync(claimPrinciple, authenticationProperty);

        //                    result = _sessions.AddSession(userInfos[0], rolesMappings, tokens, encrypteduniquekey, _Form, _sessions.UserPolicies.AllowConcurrentLogin);
        //                    _logger.LogInformation("{0} {1} {2}", "Login/AdLogin", "session added", "AdLogin");

        //                    if (result)
        //                    {
        //                        string formname = "/" + userInfos[0].DefaultFormName;

        //                        _logger.LogInformation("{0} {1} {2}", "Login/AdLogin", "User LogIn Successfully", "AdLogin");
        //                        return Redirect(formname);

        //                    }
        //                    else
        //                    {
        //                        ErrorMessage = "Session Not Started!";
        //                        _logger.LogInformation("{0} {1} {2}", "Login/AdLogin", "Session Not Started", "AdLogin");

        //                        return View();
        //                    }
        //                }
        //                else
        //                {
        //                    ErrorMessage = "Login Name Or Password Is Incorrect.";
        //                    _logger.LogInformation("{0} {1} {2}", "Login/AdLogin", "Login Name Or Password Is Incorrect.", "AdLogin");

        //                    return View();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            ErrorMessage = "Login Name Or Password Is Incorrect.";
        //            _logger.LogInformation("{0} {1} {2}", "Login/AdLogin", "Login Name Or Password Is Incorrect.", "AdLogin");

        //            return View();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogCritical("{0} {1} {2}", "Login/AdLogin", ex.Message, "AdLogin");

        //        return View();
        //    }

        //    return View();
        //}

        public async Task<IActionResult> AdLogin()
        {
            string ErrorMessage = "";
            bool result = false;
            try
            {

                string domainName = Environment.UserDomainName;
                string domainUserName = Environment.UserName;

                LoginInfo loginInfo = new LoginInfo();

                loginInfo.LoginName = domainUserName;
                loginInfo.Password = "NULL";

                nv.Clear();
                nv.Add("LoginName-varchar", loginInfo.LoginName);

                List<UserInfo> userInfos = _sessions.GetUserInfoAD(loginInfo.LoginName);


                if (userInfos.Count > 0)
                {
                    if (userInfos[0].IsAdUser)
                    {
                        loginInfo.Email = userInfos[0].UserEmail;

                        if (VerifyDomainUserExist())
                        {
                            List<RolesMapping> rolesMappings = _sessions.GetUserRole(userInfos[0].RoleID.ToString());
                            List<Navigation> navigations = _sessions.GetNavigationSettings(userInfos[0].RoleID);
                            List<Form> _Form = _commonmethods.CreateObject(navigations);
                            Tokens tokens = _sessions.GetApiToken(loginInfo.LoginName, loginInfo.Password);

                            string encrypteduniquekey = "";

                            AuthenticationProperties authenticationProperty = new AuthenticationProperties();

                            Guid g = Guid.NewGuid();
                            string uniquekey = "UserID=" + userInfos[0].UserID.ToString() + "&GUID=" + g;
                            encrypteduniquekey = _commonmethods.EncryptPassword(uniquekey);

                            if (_sessions.UserPolicies.AllowConcurrentLogin)
                            {
                                authenticationProperty.IsPersistent = true;
                            }
                            else
                            {
                                if (loginInfo.RememberMe != null)
                                {
                                    authenticationProperty.IsPersistent = true;
                                    _logger.LogInformation("{0} {2} {3}", "Login/AdLogin", "IsPersistent = true", "AdLogin");

                                }
                                else
                                {
                                    authenticationProperty.IsPersistent = false;
                                    _logger.LogInformation("{0} {2} {3}", "Login/AdLogin", "IsPersistent = False", "AdLogin");

                                }

                            }

                            var claims = new List<Claim>
                            {
                                new Claim("UserID",userInfos[0].UserID.ToString()),
                                new Claim("Guid",encrypteduniquekey),
                                new Claim(ClaimTypes.Name,userInfos[0].UserName),
                                new Claim(ClaimTypes.Email,userInfos[0].UserEmail),
                                new Claim(ClaimTypes.Role,userInfos[0].RoleID.ToString())
                            };

                            var claimIdentity = new ClaimsIdentity(claims, "IdentityClaim");
                            var claimPrinciple = new ClaimsPrincipal(claimIdentity);


                            await HttpContext.SignInAsync(claimPrinciple, authenticationProperty);

                            result = _sessions.AddSession(userInfos[0], rolesMappings, tokens, encrypteduniquekey, _Form, _sessions.UserPolicies.AllowConcurrentLogin);
                            _logger.LogInformation("{0} {2} {3}", "Login/AdLogin", "session added", "AdLogin");

                            if (result)
                            {
                                string formname = "/" + userInfos[0].DefaultFormName;

                                _logger.LogInformation("{0} {2} {3}", "Login/AdLogin", "User LogIn Successfully", "AdLogin");
                                return Redirect(formname);

                            }
                            else
                            {
                                ErrorMessage = "Session Not Started!";
                                _logger.LogInformation("{0} {2} {3}", "Login/AdLogin", "Session Not Started", "AdLogin");

                                return View();
                            }
                        }
                        else
                        {
                            ErrorMessage = "Login Name Or Password Is Incorrect.";
                            _logger.LogInformation("{0} {2} {3}", "Login/AdLogin", "Login Name Or Password Is Incorrect.", "AdLogin");

                            return View();
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Login Name Or Password Is Incorrect.";
                    _logger.LogInformation("{0} {2} {3}", "Login/AdLogin", "Login Name Or Password Is Incorrect.", "AdLogin");

                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("{0} {2} {3}", "Login/AdLogin", ex.Message, "AdLogin");

                return View();
            }

            return View();
        }
    }
}
