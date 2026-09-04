using RevenueBillingApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RevenueBillingApp.Controllers
{
    [Authorize(AuthenticationSchemes = "ASPXAUTH")]
    [TypeFilter(typeof(AuthenticationAccess))]
    public class DashboardController : Controller
    {
        public IActionResult DailyTasks()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }


    }
}
