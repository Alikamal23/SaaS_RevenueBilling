using RevenueBillingApp.Models.Authentication;
using RevenueBillingApp.Models.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace RevenueBillingApp.Utility
{
    public class AuthenticationAccess : Attribute, IAuthorizationFilter
    {
        private readonly Sessions _sessions;
        public AuthenticationAccess(Sessions sessions)
        {
            _sessions = sessions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool KickUser = false;
            bool ConfiremdLogout = true;

            ClaimsPrincipal claimsPrincipal = context.HttpContext.User;
            var UserID = (from c in claimsPrincipal.Claims where c.Type == "UserID" select c.Value).FirstOrDefault();
            var UniqueKey = (from c in claimsPrincipal.Claims where c.Type == "Guid" select c.Value).FirstOrDefault();
            if (_sessions.SessionExist(UniqueKey, UserID))
            {
                SessionItems sessionItems = _sessions.GetSession(UniqueKey, UserID);

                if (sessionItems != null)
                {
                    if (sessionItems.userInfo != null)
                    {
                        string UserID_ = sessionItems.userInfo.UserID.ToString() ?? "";

                        var routeData = context.HttpContext.Request.RouteValues;
                        var controllerName = routeData["controller"].ToString();
                        var actionName = routeData["action"].ToString();
                        var wfcode = context.HttpContext.Request.Query["wfcode"];
                        var FormName = controllerName + "/" + actionName;

                        if (!string.IsNullOrEmpty(wfcode))
                        {
                            FormName += "?wfcode=" + wfcode;
                        }
                        List<RolesMapping> permissionList = sessionItems.rolesMapping;
                        KickUser = permissionList.Any(item => item.FormName == FormName);

                        ConfiremdLogout = false;

                        _sessions.UpdateSession(UniqueKey, UserID, sessionItems.authToken);
                    }
                }
            }
            if (!KickUser)
            {
                
                if (ConfiremdLogout)
                {
                    context.Result = new RedirectResult("/Login/Logout");
                }
                else
                {
                    context.Result = new RedirectResult("/Errors/Error?statusCode=403");
                }
            }

        }
    }
}
