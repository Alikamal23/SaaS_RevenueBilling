using RevenueBillingApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RevenueBillingApp.Controllers
{
    [Authorize(AuthenticationSchemes = "ASPXAUTH")]
    [TypeFilter(typeof(AuthenticationAccess))]
    public class MasterSetupController : Controller
    {
        // DMS Landing Page (if any)
        public IActionResult Index()
        {
            return View();
        }


        // ERP Module

        #region General Setup
        public IActionResult CompanyProfile()
        {
            return View();
        }
        public IActionResult CompanyBranch()
        {
            return View();
        }

        public IActionResult FiscalYear()
        {
            return View();
        }

        public IActionResult Currency()
        {
            return View();
        }

        public IActionResult VoucherType()
        {
            return View();
        }

        public IActionResult JobOrder()
        {
            return View();
        }
        #endregion

        #region Other Master Setup
        public IActionResult ChartofAccount()
        {
            return View();
        }

        public IActionResult Supplier()
        {
            return View();
        }

        public IActionResult Customer()
        {
            return View();
        }

        public IActionResult Warehouse()
        {
            return View();
        }

        public IActionResult Manufacturer()
        {
            return View();
        }

        public IActionResult Product()
        {
            return View();
        }

        #endregion

        // Revenue & Billing Suite Module
        #region Revenue & Billing Suite Module

        //AllDropdowns
        public IActionResult AllDropdowns()
        {
            return View();
        }


        public IActionResult Client()
        {
            return View();
        }


        public IActionResult ConversionRate()
        {
            return View();
        }

        #endregion


    }
}
