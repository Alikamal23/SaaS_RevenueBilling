using RevenueBillingApp.Models.Session;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Data;

namespace RevenueBillingApp.Models.Authentication
{
    public class AllowedApiAccess : ActionFilterAttribute
    {
        private string[]? _extensions;
        private readonly Sessions _sessions;
        public AllowedApiAccess(Sessions sessions)
        {
            _sessions = sessions;
        }


        public override void OnActionExecuting(ActionExecutingContext validationContext)
        {

            bool Result = true;
            var request = validationContext.HttpContext.Request;
            var referer = validationContext.HttpContext.Request.Headers["Referer"].ToString();
            var userAgent = request.Headers["User-Agent"].ToString();
            //if (string.IsNullOrEmpty(referer) || userAgent.Contains("Postman") || userAgent.ToLower().Contains("okhttp") || userAgent.ToLower().Contains("mobile"))
            //{
            //    base.OnActionExecuting(validationContext);
            //    return;
            //}
            var refererresult = GetControllerActionFromReferer(referer);
            string? RouteValues = ((object[])validationContext.HttpContext.Request.RouteValues.Values)[1].ToString();
            ClaimsPrincipal claimsPrincipal = validationContext.HttpContext.User;
            var UserID = (from c in claimsPrincipal.Claims where c.Type == "UserID" select c.Value).FirstOrDefault();
            var UniqueKey = (from c in claimsPrincipal.Claims where c.Type == "Guid" select c.Value).FirstOrDefault();
            SessionItems sessionItems = _sessions.GetSession(UniqueKey, UserID);
            List<RolesMapping> rolesMapping = sessionItems.rolesMapping;
            //RolesMapping roleMapping = rolesMapping.Where(x => x.FormName == refererresult.Value.Controller.ToString() + "/" + refererresult.Value.Action.ToString()).FirstOrDefault();
            string formName = refererresult.Value.Controller.ToString() + "/" + refererresult.Value.Action.ToString();
            RolesMapping roleMapping = rolesMapping.FirstOrDefault(item => item.FormName.Contains(formName, StringComparison.OrdinalIgnoreCase));
            var _apimaster = (roleMapping.ApiMaster !=null) ? roleMapping.ApiMaster.Where(y => y.ApiAction == RouteValues).Where(z => z.IsAllowed).FirstOrDefault() : null;

            if (_apimaster != null) {

                if (!_apimaster.IsAllowed)
                {
                    Result = false;
                }
            }
            else
            {
                Result = false;
            }


            if (!Result)
            {
                validationContext.Result = new BadRequestObjectResult(GetErrorMessage());
            }
        }

        public string GetErrorMessage()
        {
            return $"This Api is not allowed! Please Contact System Adminstrator";
        }
        private (string Controller, string Action)? GetControllerActionFromReferer(string referer)
        {
            if (Uri.TryCreate(referer, UriKind.Absolute, out var uri))
            {
                var path = uri.AbsolutePath;
                var segments = path.Trim('/').Split('/');

                if (segments.Length >= 2)
                {
                    return (segments[0], segments[1]);
                }
            }

            return null;
        }
    }
}
