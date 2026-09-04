using RevenueBillingApp.Controllers;
using RevenueBillingApp.Models.Authentication;
using RevenueBillingApp.Models.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace RevenueBillingApp.Middleware
{
    public class RequestClient
    {
        private readonly Sessions _sessions;
        private readonly ILogger<RequestClient> _logger;
        private readonly HttpContext _httpContext;
        public RequestClient(Sessions sessions, ILogger<RequestClient> logger, IHttpContextAccessor httpContextAccessor) 
        {
            _sessions = sessions;
            _logger = logger;
            _httpContext = httpContextAccessor.HttpContext;
        }
        #region Get Session

        private SessionItems GetSessionItems(HttpContext httpContext)
        {
            ClaimsPrincipal claimsPrincipal = httpContext.User;
            string HostName = Dns.GetHostName();
            IPHostEntry HostIPs = Dns.GetHostEntry(HostName);
            string IPAddress = HostIPs.AddressList[0].ToString();
            var UniqueKey = (from c in claimsPrincipal.Claims where c.Type == "Guid" select c.Value).FirstOrDefault();
            var UserID = (from c in claimsPrincipal.Claims where c.Type == "UserID" select c.Value).FirstOrDefault();
            SessionItems session = _sessions.GetSession(UniqueKey, UserID);
            session.IpAddress = IPAddress;
            return session;
        }

        public UserInfo GetUserInformation()
        {
            UserInfo tlistFiltered = new UserInfo();

            try
            {
                SessionItems session = GetSessionItems(_httpContext);
                _logger.LogInformation("{0} {1} UserID : {2} IPAddress : {4}", "BaseController", "GetUserInformation", session.userInfo.UserID, session.IpAddress);
                tlistFiltered = session.userInfo;

            }
            catch (Exception ex)
            {
                _logger.LogCritical("BaseController/GetUserInformation {1}", ex.Message);
            }

            return tlistFiltered;
        }


        public List<RolesMapping> GetSessionForHtml(string RouteValues)
        {
            List<RolesMapping> tlistFiltered = new List<RolesMapping>();

            try
            {
                SessionItems session = GetSessionItems(_httpContext);
                List<RolesMapping> permissionList = session.rolesMapping;
                tlistFiltered = permissionList.Where(item => "/" + item.FormName == RouteValues).ToList();
                _logger.LogInformation("{0} {1} UserID : {2} {3} IPAddress : {4}", "BaseController", "GetSession", session.userInfo.UserID, RouteValues, session.IpAddress);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("BaseController/GetSession {1}", ex.Message);
            }

            return tlistFiltered;
        }
        #endregion

        #region Get Token
        private string GetAuthToken(HttpContext httpContext)
        {
            SessionItems sessionItems = GetSessionItems(httpContext);
            string authToken = sessionItems.authToken.access_token;
            _logger.LogInformation("{0} {1} UserID : {2} AuthToken :{3} IPAddress : {4}", "BaseController", "GetAuthToken", sessionItems.userInfo.UserID, authToken, sessionItems.IpAddress);
            return authToken;
        }
        #endregion

        #region HttpClient GET
        public HttpResponseMessage UseHttpClientGet(string? QueryString, string ApiName, string ControllerName)
        {
            HttpResponseMessage Result_ = new HttpResponseMessage();
            try
            {
                using (var client = new HttpClient())
                {
                    string authtoken = GetAuthToken(_httpContext);
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL"));
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authtoken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var getTask = client.GetAsync(ControllerName + "/" + ApiName + ((QueryString == null) ? "" : QueryString));
                    getTask.Wait();
                    Result_ = getTask.Result;
                    _logger.LogInformation("{0} {1} APIName: {2} StatusCode :{3}", "BaseController", "GET", ControllerName + "/" + ApiName, getTask.Result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("BaseController/UseHttpClientGet {1} {2}", ex.Message, ApiName);
            }
            return Result_;
        }
        #endregion

        #region HttpClient POST
        public HttpResponseMessage UseHttpClientPost(string Data, string ApiName, string ControllerName)
        {
            HttpResponseMessage Result_ = new HttpResponseMessage();
            try
            {
                using (var client = new HttpClient())
                {
                    string authtoken = GetAuthToken(_httpContext);
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + ControllerName + "/" + ApiName);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authtoken);
                    var stringContent = new StringContent(Data, Encoding.UTF8, "application/json");
                    var postTask = client.PostAsync(ApiName, stringContent);
                    postTask.Wait();
                    Result_ = postTask.Result;
                    _logger.LogInformation("{0} {1} APIName: {2} StatusCode :{3}", "BaseController", "POST", ControllerName + "/" + ApiName, postTask.Result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("BaseController/UseHttpClientPost {1} {2}", ex.Message, ApiName);
            }
            return Result_;
        }
        #endregion

    }
}
