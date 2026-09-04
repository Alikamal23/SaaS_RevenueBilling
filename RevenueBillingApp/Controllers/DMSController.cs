using RevenueBillingApp.Models.Authentication;
using RevenueBillingApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RevenueBillingApp.Controllers
{
    [Authorize(AuthenticationSchemes = "ASPXAUTH")]
    [TypeFilter(typeof(AuthenticationAccess))]
    public class DMSController : Controller
    {
        // DMS Landing Page (if any)
        public IActionResult Index()
        {
            return View();
        }

        #region DMS MasterForm
        public IActionResult KeyFactors()
        {
            return View();
        }

        public IActionResult SPODealerUpload()
        {
            return View();
        }

        public IActionResult MasterDealer()
        {
            return View();
        }
        #endregion

        #region DMS Forms
        public IActionResult KPIForm()
        {
            return View();
        }
        public IActionResult OSForm()
        {
            return View();
        }
        public IActionResult CWMForm()
        {
            return View();
        }
        public IActionResult DFQForm()
        {
            return View();
        }
        #endregion

        #region DMS Survey Forms
        public IActionResult KPISurveyForm()
        {
            return View();
        }
        #endregion


    }
}
