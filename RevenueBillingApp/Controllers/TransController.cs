using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.Contracts;

namespace RevenueBillingApp.Controllers
{
    public class TransController : Controller
    {
        // All Page in One Tab wise
        public IActionResult Index()
        {
            return View();
        }

        #region Transaction - ERP Module
        public IActionResult DebitVoucher()
        {
            return View();
        }

        public IActionResult CreditVoucher()
        {
            return View();
        }

        public IActionResult JournalVoucher()
        {
            return View();
        }


        public IActionResult PurchaseOrder()
        {
            return View();
        }
        #endregion

        #region Transaction - SaaS Module

        #region All Forms Add/Edit

        public IActionResult ClientOnBoarding(int? ClientId)
        {
            ViewBag.ClientId = ClientId;
            return View();
        }

        public IActionResult ContractForm(int? ContractId)
        {
            ViewBag.ContractId = ContractId;
            return View();
        }

        public IActionResult BillingProfile(int? BillingId)
        {
            ViewBag.BillingId = BillingId;
            return View();
        }

        public IActionResult RecurringBilling(int? RBillingId)
        {
            ViewBag.RBillingId = RBillingId;
            return View();
        }

        public IActionResult MilestoneForm(int? MilestoneId)
        {
            ViewBag.MilestoneId = MilestoneId;
            return View();
        }

        public IActionResult MilestoneApprovalForm(int? MilestoneId)
        {
            ViewBag.MilestoneId = MilestoneId;
            return View();
        }

        public IActionResult InvoiceForm(int? InvoiceId)
        {
            ViewBag.InvoiceId = InvoiceId;
            return View();
        }

        public IActionResult PaymentForm(int? PaymentId)
        {
            ViewBag.PaymentId = PaymentId;
            return View();
        }

        public IActionResult ARExceptionForm(int? ExceptionId)
        {
            ViewBag.ExceptionId = ExceptionId;
            return View();
        }

        public IActionResult RenewalForm(int? RenewId)
        {
            ViewBag.RenewId = RenewId;
            return View();
        }

        #endregion

        #region All View Grid

        public IActionResult ViewClientOnBoarding()
        {
            return View();
        }

        public IActionResult ViewContractForm()
        {
            return View();
        }

        public IActionResult ViewBillingProfile()
        {
            return View();
        }

        public IActionResult ViewRecurringBilling()
        {
            return View();
        }

        public IActionResult ViewMilestoneForm()
        {
            return View();
        }

        public IActionResult ViewMilestoneApprovalForm()
        {
            return View();
        }

        public IActionResult ViewInvoiceForm()
        {
            return View();
        }

        public IActionResult ViewPaymentForm()
        {
            return View();
        }

        public IActionResult ViewARExceptionForm()
        {
            return View();
        }

        public IActionResult ViewRenewalForm()
        {
            return View();
        }






        #endregion

        #endregion

    }
}
