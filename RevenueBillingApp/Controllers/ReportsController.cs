using RevenueBillingApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RevenueBillingApp.Controllers
{
    [Authorize(AuthenticationSchemes = "ASPXAUTH")]
    [TypeFilter(typeof(AuthenticationAccess))]
    public class ReportsController : Controller
    {
        // DMS Landing Page (if any)
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ClientonBoarding()
        {
            return View();
        }

        public IActionResult rpt_ClientonBoarding()
        {
            return View();
        }

        //#region General Setup
        //public IActionResult CompanyProfile()
        //{
        //    return View();
        //}

        //public IActionResult FiscalYear()
        //{
        //    return View();
        //}

        //public IActionResult Currency()
        //{
        //    return View();
        //}

        //public IActionResult VoucherType()
        //{
        //    return View();
        //}

        //public IActionResult JobOrder()
        //{
        //    return View();
        //}
        //#endregion




    }
}
