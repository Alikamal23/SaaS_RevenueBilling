using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace RevenueBillingApp.Controllers
{
    public class BillingRevenueController : Controller
    {
        private readonly IConfiguration _configuration;

        public BillingRevenueController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public IActionResult ClientOnboarding()
        {
            return View();
        }
        public IActionResult ClientList()
        {
            return View();
        }

        public IActionResult ClientDashboard()
        {
            return View();
        }

        public IActionResult CreateInvoice()
        {
            return View();
        }

        public IActionResult rpt_ClientInformation(int ClientId)
        {
            ViewBag.ClientId = ClientId;
            return View();
        }

        //AutoInvoice
        public async Task<IActionResult> AutoInvoice()
        {
            try
            {
                string ApiServiceURL = _configuration["ApiSettings:BaseUrl"];
                bool enabled = _configuration.GetValue<bool>("AutoInvoice:Enabled");

                if (!enabled)
                {
                    TempData["AutoInvoiceError"] = "Auto Invoice is disabled.";
                    return RedirectToAction("ClientDashboard", "BillingRevenue");
                }

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiServiceURL); //new Uri(configuration["ApiSettings:BaseUrl"]);

                    var response = await client.PostAsync("BillingRevenue/AutoGenerateInvoice", new StringContent("", Encoding.UTF8, "application/json"));

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        TempData["AutoInvoiceSuccess"] = "Auto Invoice executed successfully.";
                        return RedirectToAction("ClientDashboard", "BillingRevenue");
                    }

                    TempData["AutoInvoiceError"] = "API Error: " + response.StatusCode;
                    return RedirectToAction("ClientDashboard", "BillingRevenue");

                }
            }
            catch (Exception ex)
            {
                ////return BadRequest(new { success = false, message = "Error: " + ex.Message });

                TempData["AutoInvoiceError"] = "Error: " + ex.Message;
                return RedirectToAction("ClientDashboard", "BillingRevenue");
            }
        }

        //NextUnpaidInvoiceDue

        public IActionResult NextUnpaidInvoiceDue()
        {
            return View();
        }

        //EmailTemplate
        public IActionResult EmailTemplate()
        {
            return View();
        }


        //Reports .....
        #region Reports Menu
        public IActionResult ClientReport()
        {
            return View();
        }

        public IActionResult ClientContractReport()
        {
            return View();
        }


        public IActionResult ClientInvoiceReport()
        {
            return View();
        }


        public IActionResult InvoiceDueReport()
        {
            return View();
        }



        //rpt_ClientList
        //rpt_ClientContractList
        //rpt_ClientInvoiceList
        //rpt_InvoiceDueReport

        public IActionResult rpt_ClientReport()
        {
            return View();
        }

        public IActionResult rpt_ClientContractReport()
        {
            return View();
        }


        public IActionResult rpt_ClientInvoiceReport()
        {
            return View();
        }


        public IActionResult rpt_InvoiceDueReport()
        {
            return View();
        }

        //One Page ReportDashboard
        //ReportDashboard
        public IActionResult ReportDashboard()
        {
            return View();
        }



        #endregion


    }
}
