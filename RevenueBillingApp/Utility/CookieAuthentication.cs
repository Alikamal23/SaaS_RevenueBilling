using RevenueBillingApp.Models.Session;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace RevenueBillingApp.Utility
{
    public class CookieAuthentication : CookieAuthenticationEvents
    {
        private readonly Sessions _sessions;
        public CookieAuthentication(Sessions sessions)
        {
            _sessions = sessions;
        }
        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {

            var userPrincipal = context.Principal;

            var UserID = (from c in userPrincipal.Claims where c.Type == "UserID" select c.Value).FirstOrDefault();
            var UniqueKey = (from c in userPrincipal.Claims where c.Type == "Guid" select c.Value).FirstOrDefault();

            SessionItems sessionItems = _sessions.GetSession(UniqueKey, UserID);

            if (sessionItems == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync("ASPXAUTH");
                _sessions.RemoveSession(UniqueKey, UserID);
            }
        }
    }
}
