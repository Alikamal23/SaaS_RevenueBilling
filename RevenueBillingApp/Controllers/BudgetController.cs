using RevenueBillingApp.Middleware;
using RevenueBillingApp.Models.Authentication;
using RevenueBillingApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Reflection.Emit;

namespace RevenueBillingApp.Controllers
{
    [Authorize(AuthenticationSchemes = "ASPXAUTH")]
    [TypeFilter(typeof(AuthenticationAccess))]
    public class BudgetController : Controller
    {
        private readonly RequestClient _requestClient;

        public BudgetController(RequestClient requestClient)
        {
            _requestClient = requestClient;
        }

        public async Task<IActionResult> Initiate(int instanceid = 0)
        {
            ViewBag.wfCode = "Budget";
            if (instanceid > 0)
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                HttpResponseMessage response = _requestClient.UseHttpClientGet("?instanceid=" + instanceid + "&roleid=" + userinfo.RoleID + "&userid=" + userinfo.UserID, "IsTaskAllowed", "Workflow");
                if (response.IsSuccessStatusCode)
                {
                    var resultContent = await response.Content.ReadAsStringAsync();

                    bool isAllowed = resultContent.Trim() == "true";

                    if (isAllowed)
                    {
                        return View();
                    }
                }
                return Redirect("/" + userinfo.DefaultFormName);
            }
            else
            {
                return View();
            }
        }
        public IActionResult MyTask()
        {
            return View();
        }
        public IActionResult MyRequest()
        {
            return View();
        }
        public IActionResult MyApproval()
        {
            return View();
        }
        public IActionResult ViewMyRequest(int instanceid = 0)
        {
            return View();
        }

    }
}
