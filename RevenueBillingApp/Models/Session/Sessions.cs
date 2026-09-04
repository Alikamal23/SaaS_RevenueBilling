using System.Runtime.Caching;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Data;
using System.Reflection;
using Newtonsoft.Json;
using RevenueBillingApp.Models.Authentication;

namespace RevenueBillingApp.Models.Session
{
    public class Sessions
    {
        public static List<SessionItems> cache_ = (List<SessionItems>)MemoryCache.Default["_LoginUsersEmail"];
        public static UserPolicies UserPoliciesInstance = null;
        public static List<ErrorInfo> ErrorInfoInstance = null;
        private readonly ILogger<Sessions> _logger;

        public UserPolicies UserPolicies
        {
            get
            {
                if (UserPoliciesInstance == null)
                {
                    UserPoliciesInstance = GetUserPolicies()[0];
                }
                return UserPoliciesInstance;
            }
        }

        public List<ErrorInfo> ErrorInfo
        {
            get
            {
                if (ErrorInfoInstance == null)
                {
                    ErrorInfoInstance = GetErrorInfo();
                }
                return ErrorInfoInstance;
            }
        }

        private readonly IConfiguration _configuration;
        public Sessions(IConfiguration configuration, ILogger<Sessions> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public List<UserPolicies> GetUserPolicies()
        {
            List<UserPolicies> userPolicies = null;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/FillPasswordPolicy");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/FillPasswordPolicy");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("FillPasswordPolicy", stringContent);
                    postTask.Wait();
                    var Result = postTask.Result;
                    if (Result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/FillPasswordPolicy", postTask.Status);
                        string resultmessage = Result.Content.ReadAsStringAsync().Result;
                        userPolicies = JsonConvert.DeserializeObject<List<UserPolicies>>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/FillPasswordPolicy", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/FillPasswordPolicy", ex.Message);
            }
            return userPolicies;
        }
        public List<ErrorInfo> GetErrorInfo()
        {
            List<ErrorInfo> ErrorInfo = null;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetErrorInfo");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/GetErrorInfo");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("GetErrorInfo", stringContent);
                    postTask.Wait();
                    var Result = postTask.Result;
                    if (Result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetErrorInfo", postTask.Status);
                        string resultmessage = Result.Content.ReadAsStringAsync().Result;
                        ErrorInfo = JsonConvert.DeserializeObject<List<ErrorInfo>>(resultmessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetErrorInfo", ex.Message);
            }
            return ErrorInfo;
        }

        public bool SessionExist(string UniqueKey, string UserID)
        {
            bool exist_ = false;

            try
            {
                if (UserPolicies.AllowConcurrentLogin)
                {
                    _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = True");
                    exist_ = cache_.Any(c => c.UniqueKey == UniqueKey);
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = False");
                    List<SessionItems> n_cache_ = MemoryCache.Default["_LoginUsersEmail_" + UserID] == null ? new List<SessionItems>() : (List<SessionItems>)MemoryCache.Default["_LoginUsersEmail_" + UserID];
                    exist_ = n_cache_.Any(c => c.UniqueKey == UniqueKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            return exist_;
        }

        public bool AddSession(UserInfo userInfo, List<RolesMapping> rolesMapping, Tokens authToken, string UniqueKey, List<Form> _Form, bool AllowConcurrentLogin)
        {

            NameValueCollection nv = new NameValueCollection();
            bool performed_ = false;
            SessionItems items = new SessionItems { userInfo = userInfo, rolesMapping = rolesMapping, authToken = authToken, UniqueKey = UniqueKey , forms = _Form };

            try
            {
                if (AllowConcurrentLogin)
                {
                    if (cache_ == null)
                    {
                        _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = True , cache_ == null");
                        cache_ = new List<SessionItems>();
                        cache_.Add(items);
                        MemoryCache.Default["_LoginUsersEmail"] = cache_;
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = True , cache_ != null");
                        cache_.Add(items);
                    }
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = False");

                    List<SessionItems> n_cache_ = new List<SessionItems>();
                    n_cache_.Add(items);
                    MemoryCache.Default["_LoginUsersEmail_" + userInfo.UserID] = n_cache_;
                }

                using (var client = new HttpClient())
                {
                    _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/InsertUserSession");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{UserID:'" + userInfo.UserID.ToString() + "', SessionID: '" + UniqueKey + "', IsLogin:'1'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("InsertUserSession", stringContent);
                    postTask.Wait();
                    var Result = postTask.Result;
                    if (Result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession", postTask.Status);
                        performed_ = true;
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession", postTask.Status);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession", ex.Message);
            }

            return performed_;
        }


        //public bool AddSession(UserInfo userInfo, List<RolesMapping> rolesMapping, Tokens authToken, string UniqueKey, List<Form> _Form, bool AllowConcurrentLogin)
        //{

        //    NameValueCollection nv = new NameValueCollection();
        //    bool performed_ = false;
        //    SessionItems items = new SessionItems { userInfo = userInfo, rolesMapping = rolesMapping, authToken = authToken, UniqueKey = UniqueKey, forms = _Form };

        //    try
        //    {
        //        if (AllowConcurrentLogin)
        //        {
        //            if (cache_ == null)
        //            {
        //                _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = True , cache_ == null");
        //                cache_ = new List<SessionItems>();
        //                cache_.Add(items);
        //                MemoryCache.Default["_LoginUsersEmail"] = cache_;
        //            }
        //            else
        //            {
        //                _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = True , cache_ != null");
        //                cache_.Add(items);
        //            }
        //        }
        //        else
        //        {
        //            _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = False");

        //            List<SessionItems> n_cache_ = new List<SessionItems>();
        //            n_cache_.Add(items);
        //            MemoryCache.Default["_LoginUsersEmail_" + userInfo.UserID] = n_cache_;
        //        }

        //        using (var client = new HttpClient())
        //        {
        //            _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession");
        //            client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/InsertUserSession");
        //            client.DefaultRequestHeaders.Clear();
        //            string content = "{UserID:'" + userInfo.UserID.ToString() + "', SessionID: '" + UniqueKey + "', IsLogin:'1'}";
        //            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
        //            var postTask = client.PostAsync("InsertUserSession", stringContent);
        //            postTask.Wait();
        //            var Result = postTask.Result;
        //            if (Result.IsSuccessStatusCode)
        //            {
        //                _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession", postTask.Status);
        //                performed_ = true;
        //            }
        //            else
        //            {
        //                _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession", postTask.Status);

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession", ex.Message);
        //    }

        //    return performed_;
        //}

        public bool RemoveSession(string UniqueKey, string userid)
        {
            bool performed_ = false;
            try
            {
                if (UserPolicies.AllowConcurrentLogin)
                {
                    _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = True");
                    cache_.Remove(cache_.Where(c => c.UniqueKey == UniqueKey).FirstOrDefault());
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = False");
                    List<SessionItems> n_cache_ = MemoryCache.Default["_LoginUsersEmail_" + userid] == null ? new List<SessionItems>() : (List<SessionItems>)MemoryCache.Default["_LoginUsersEmail_" + userid];
                    n_cache_.Clear();
                }

                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession");

                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/InsertUserSession");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{UserID:'" + userid.ToString() + "', SessionID: 'NULL', IsLogin:'0'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("InsertUserSession", stringContent);
                    postTask.Wait();
                    var Result = postTask.Result;
                    if (Result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession", postTask.Status);
                        performed_ = true;
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession", postTask.Status);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/InsertUserSession", ex.Message);

            }
            return performed_;
        }

        public SessionItems GetSession(string UniqueKey, string UserID)
        {
            SessionItems sessionItems;

            try
            {
                if (UserPolicies.AllowConcurrentLogin)
                {
                    sessionItems = cache_.Where(c => c.UniqueKey == UniqueKey).FirstOrDefault();
                }
                else
                {
                    List<SessionItems> n_cache_ = MemoryCache.Default["_LoginUsersEmail_" + UserID] == null ? new List<SessionItems>() : (List<SessionItems>)MemoryCache.Default["_LoginUsersEmail_" + UserID];
                    sessionItems = n_cache_.Where(c => c.UniqueKey == UniqueKey).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                sessionItems = new SessionItems();
                _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return sessionItems;
        }

        public bool UpdateSession(string UniqueKey, string UserID, Tokens authToken)
        {
            bool performed = false;
            try
            {

                if (UserPolicies.AllowConcurrentLogin)
                {
                    _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = True");
                    cache_.Where(c => c.UniqueKey == UniqueKey).FirstOrDefault().authToken = authToken;
                }
                else
                {
                    _logger.LogInformation("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "AllowConcurrentLogin = False");
                    List<SessionItems> n_cache_ = MemoryCache.Default["_LoginUsersEmail_" + UserID] == null ? new List<SessionItems>() : (List<SessionItems>)MemoryCache.Default["_LoginUsersEmail_" + UserID];
                    n_cache_.Find(n => n.UniqueKey == UniqueKey).authToken = authToken;
                }
                performed = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return performed;
        }

        public bool CheckLoginCreds(string LoginName, string Password)
        {
            bool result = false;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckLoginCreds");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/CheckLoginCreds");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{LoginName:'" + LoginName + "', UserPassword: '" + Password + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("CheckLoginCreds", stringContent);
                    postTask.Wait();
                    var Result = postTask.Result;
                    if (Result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckLoginCreds", postTask.Status);

                        string resultmessage = Result.Content.ReadAsStringAsync().Result;
                        result = JsonConvert.DeserializeObject<bool>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckLoginCreds", postTask.Status);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckLoginCreds", ex.Message);

            }
            return result;
        }

        public List<UserInfo>? GetUserInfo(string LoginName, string Password)
        {
            List<UserInfo>? userInfo = null;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserInfo");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/GetUserInfo");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{LoginName:'" + LoginName + "', UserPassword: '" + Password + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("GetUserInfo", stringContent);
                    postTask.Wait();
                    var Result = postTask.Result;
                    if (Result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserInfo", postTask.Status);

                        string resultmessage = Result.Content.ReadAsStringAsync().Result;
                        userInfo = JsonConvert.DeserializeObject<List<UserInfo>>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserInfo", postTask.Status);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserInfo", ex.Message);
            }
            return userInfo;
        }
        public List<UserInfo>? GetUserInfoAD(string LoginName)
        {
            List<UserInfo>? userInfo = null;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserInfoAD");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/GetUserInfoAD");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{LoginName:'" + LoginName + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("GetUserInfoAD", stringContent);
                    postTask.Wait();
                    var Result = postTask.Result;
                    if (Result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserInfoAD", postTask.Status);
                        string resultmessage = Result.Content.ReadAsStringAsync().Result;
                        userInfo = JsonConvert.DeserializeObject<List<UserInfo>>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserInfoAD", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserInfoAD", ex.Message);

            }
            return userInfo;
        }

        public List<RolesMapping>? GetUserRole(string RoleID)
        {
            List<RolesMapping>? rolesMappings = null;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserRole");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/GetUserRole");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{RoleID:'" + RoleID + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("GetUserRole", stringContent);
                    postTask.Wait();
                    var Result = postTask.Result;
                    if (Result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserRole", postTask.Status);

                        string resultmessage = Result.Content.ReadAsStringAsync().Result;
                        rolesMappings = JsonConvert.DeserializeObject<List<RolesMapping>>(resultmessage);

                        rolesMappings = rolesMappings.Select(x =>
                        {
                            x.ApiMaster = (x.JSONData != null ? JsonConvert.DeserializeObject<List<APIMaster>>(x.JSONData) : null); 
                            return x;
                        }).ToList();
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserRole", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserRole", ex.Message);

            }
            return rolesMappings;
        }

        public Tokens GetApiToken(string LoginName, string Password)
        {
            Tokens token = new Tokens();

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Authentication/token");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Authentication/token");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{LoginName:'" + LoginName + "', UserPassword: '" + Password + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("token", stringContent);
                    postTask.Wait();
                    var Result = postTask.Result;
                    if (Result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Authentication/token", postTask.Status);
                        var resultmessage = Result.Content.ReadAsStringAsync().Result;
                        token = JsonConvert.DeserializeObject<Tokens>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Authentication/token", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "auth/token", ex.Message);
            }

            return token;
        }

        public bool ResetUserAttemptCount(string LoginName)
        {
            bool Result = false;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ResetUserAttemptCount");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/ResetUserAttemptCount");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{LoginName:'" + LoginName + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("ResetUserAttemptCount", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ResetUserAttemptCount", postTask.Status);

                        string resultmessage = Result_.Content.ReadAsStringAsync().Result;
                        Result = JsonConvert.DeserializeObject<bool>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ResetUserAttemptCount", postTask.Status);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ResetUserAttemptCount", ex.Message);

            }
            return Result;
        }

        public string UpdateUserAttemptCount(string LoginName)
        {
            string Result = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/UpdateUserAttemptCount");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/UpdateUserAttemptCount");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{LoginName:'" + LoginName + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("UpdateUserAttemptCount", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/UpdateUserAttemptCount", postTask.Status);

                        string resultmessage = Result_.Content.ReadAsStringAsync().Result;
                        Result = JsonConvert.DeserializeObject<string>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/UpdateUserAttemptCount", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/UpdateUserAttemptCount", ex.Message);
            }
            return Result;
        }

        public void SystemActivityLog(int? FormID, int? ActivityID, int? UserID, string ActivityDetails)
        {
            bool Result = false;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/SystemActivityLog");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/SystemActivityLog");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{FormID:'" + FormID + "', ActivityID: '" + ActivityID + "',UserID:'" + UserID + "',ActivityDetails:'" + ActivityDetails + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("SystemActivityLog", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/SystemActivityLog", postTask.Status);
                        string resultmessage = Result_.Content.ReadAsStringAsync().Result;
                        Result = JsonConvert.DeserializeObject<bool>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/SystemActivityLog", postTask.Status);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/SystemActivityLog", ex.Message);
            }
        }

        public List<PasswordHistory>? CheckUserPassword(string? userid)
        {
            List<PasswordHistory>? Result = new List<PasswordHistory>();

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckUserPassword");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/CheckUserPassword");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{UserID:'" + userid + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("CheckUserPassword", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode && Result_.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckUserPassword", postTask.Status);
                        string ResultMessage = Result_.Content.ReadAsStringAsync().Result;
                        Result = JsonConvert.DeserializeObject<List<PasswordHistory>>(ResultMessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckUserPassword", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckUserPassword", ex.Message);
            }
            return Result;
        }

        public string? GetUserHashPassword(string? LoginName)
        {
            string? Result = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserHashPassword");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/GetUserHashPassword");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{LoginName: '" + LoginName + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("GetUserHashPassword", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode && Result_.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserHashPassword", postTask.Status);
                        Result = Result_.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserHashPassword", postTask.Status);
                        Result = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserHashPassword", ex.Message);

            }
            return Result;
        }

        public string CheckOldPassword(string? UserID, string? userpassword)
        {
            string Result = "";

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckOldPassword");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/CheckOldPassword");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{UserID: '" + UserID + "' , UserPassword: '" + userpassword + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("CheckOldPassword", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckOldPassword", postTask.Status);
                        Result = Result_.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckOldPassword", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/CheckOldPassword", ex.Message);

            }
            return Result;
        }

        public string ChangePassword_Login(string UserID, string UserPassword_New, string UserPassword_Old)
        {
            string Result = "";

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ChangePassword_Login");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/ChangePassword_Login");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{UserID:'" + UserID + "',UserPassword_New:'" + UserPassword_New + "',UserPassword_Old:'" + UserPassword_Old + "'}";

                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("ChangePassword_Login", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ChangePassword_Login", postTask.Status);

                        Result = Result_.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ChangePassword_Login", postTask.Status);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ChangePassword_Login", ex.Message);

            }
            return Result;
        }

        public VerifyEmail GetUserEmailVerification(string? UserEmail)
        {
            VerifyEmail Result = new VerifyEmail();

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserEmailVerification");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/GetUserEmailVerification");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{UserEmail: '" + UserEmail + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("GetUserEmailVerification", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode && Result_.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserEmailVerification", postTask.Status);
                        string resultmessage = Result_.Content.ReadAsStringAsync().Result;
                        Result = JsonConvert.DeserializeObject<List<VerifyEmail>>(resultmessage)[0];
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserEmailVerification", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetUserEmailVerification", ex.Message);
            }
            return Result;
        }

        public bool ResetUserPassword(string? LoginName, string? UserEmail, string UserPassword)
        {
            bool Result = false;

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ResetUserPassword");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/ResetUserPassword");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{LoginName: '" + LoginName + "',UserEmail: '" + UserEmail + "',UserPassword:'" + UserPassword + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("ResetUserPassword", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode && Result_.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ResetUserPassword", postTask.Status);
                        string resultmessage = Result_.Content.ReadAsStringAsync().Result;
                        Result = JsonConvert.DeserializeObject<bool>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ResetUserPassword", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/ResetUserPassword", ex.Message);
            }
            return Result;
        }

        public List<SMTPSettings> GetSMTPSettings()
        {
            List<SMTPSettings> Result = new List<SMTPSettings>();

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetSMTPSettings");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/GetSMTPSettings");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("GetSMTPSettings", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode && Result_.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetSMTPSettings", postTask.Status);
                        string resultmessage = Result_.Content.ReadAsStringAsync().Result;
                        Result = JsonConvert.DeserializeObject<List<SMTPSettings>>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetSMTPSettings", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetSMTPSettings", ex.Message);
            }
            return Result;
        }

        public List<Navigation> GetNavigationSettings(int? RoleID)
        {
            List<Navigation> Result = new List<Navigation>();

            try
            {
                using (var client = new HttpClient())
                {
                    _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetNavigationSettings");
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/GetNavigationSettings");
                    client.DefaultRequestHeaders.Clear();
                    string content = "{RoleID: '" + RoleID + "'}";
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync("GetNavigationSettings", stringContent);
                    postTask.Wait();
                    var Result_ = postTask.Result;
                    if (Result_.IsSuccessStatusCode && Result_.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetNavigationSettings", postTask.Status);
                        string resultmessage = Result_.Content.ReadAsStringAsync().Result;
                        Result = JsonConvert.DeserializeObject<List<Navigation>>(resultmessage);
                    }
                    else
                    {
                        _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetNavigationSettings", postTask.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetNavigationSettings", ex.Message);
            }
            return Result;
        }

        //public List<APIMaster> GetApiMaster(int? RoleID)
        //{
        //    List<APIMaster> Result = new List<APIMaster>();

        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            _logger.LogError("{0} {1} {2}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetApiMaster");
        //            client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/GetApiMaster");
        //            client.DefaultRequestHeaders.Clear();
        //            string content = "{RoleID: '" + RoleID + "'}";
        //            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
        //            var postTask = client.PostAsync("GetApiMaster", stringContent);
        //            postTask.Wait();
        //            var Result_ = postTask.Result;
        //            if (Result_.IsSuccessStatusCode && Result_.StatusCode == HttpStatusCode.OK)
        //            {
        //                _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetApiMaster", postTask.Status);
        //                string resultmessage = Result_.Content.ReadAsStringAsync().Result;
        //                Result = JsonConvert.DeserializeObject<List<APIMaster>>(resultmessage);
        //            }
        //            else
        //            {
        //                _logger.LogInformation("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetApiMaster", postTask.Status);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("{0} {1} {2} {3}", "Sessions", MethodBase.GetCurrentMethod().Name, "Sessions/GetApiMaster", ex.Message);
        //    }
        //    return Result;
        //}
    }
}
