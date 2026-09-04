using RevenueBillingApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RevenueBillingApp.Controllers
{
    [Authorize(AuthenticationSchemes = "ASPXAUTH")]
    [TypeFilter(typeof(AuthenticationAccess))]
    public class SettingsController : Controller
    {
        public IActionResult UserMaster()
        {
            return View();
        }
        public IActionResult RolesMaster()
        {
            return View();
        }
        public IActionResult SystemPriviliges()
        {
            return View();
        }
        public IActionResult SmtpSetting()
        {
            return View();
        }
        public IActionResult UserProfile()
        {
            return View();
        }
        public IActionResult OrganizationChart()
        {
            return View();
        }
    }
}
