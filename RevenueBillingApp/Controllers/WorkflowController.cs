using RevenueBillingApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RevenueBillingApp.Controllers
{
    [Authorize(AuthenticationSchemes = "ASPXAUTH")]
    [TypeFilter(typeof(AuthenticationAccess))]
    public class WorkflowController : Controller
    {
        public IActionResult Workflows()
        {
            return View();
        }
        public IActionResult WorkflowSteps()
        {
            return View();
        }

        public IActionResult WorkFlowStepAction()
        {
            return View();
        }
        public IActionResult MyTask(string wfcode)
        {
            return View();
        }
        public IActionResult MyApproval(string wfcode)
        {
            return View();
        }
        public IActionResult MyRequest(string wfcode)
        {
            return View();
        }

    }
}
