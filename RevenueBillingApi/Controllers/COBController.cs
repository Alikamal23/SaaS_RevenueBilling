using RevenueBillingApi.Context;
using RevenueBillingApi.RateLimiting;
using RevenueBillingApi.Utility;
using RevenueBillingApi.Models.COB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Data;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Web;
using Microsoft.IdentityModel.Tokens;

namespace RevenueBillingApi.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/{controller}/{action}/{id:int?}")]
    [ApiController]
    public class COBController : ControllerBase
    {
        private readonly DataAccessLayer _DAL;
        private readonly ILogger<SettingsController> _logger;
        public COBController(DataAccessLayer DAL, ILogger<SettingsController> logger)
        {
            _DAL = DAL;
            _logger = logger;
        }

        #region Activity Log
        private void SystemActivityLog(int? ActivityID, string? ActivityDetails)
        {
            bool Result = false;

            ClaimsPrincipal claimsPrincipal = HttpContext.User;
            string HostName = Dns.GetHostName();
            IPHostEntry HostIPs = Dns.GetHostEntry(HostName);
            string IPAddress = HostIPs.AddressList[0].ToString();
            string UserID = (from c in claimsPrincipal.Claims where c.Type == "UserID" select c.Value).FirstOrDefault();
            var routeData = HttpContext.Request.RouteValues;
            string controllerName = routeData["controller"].ToString();
            string actionName = routeData["action"].ToString();
            string FormName = controllerName + "/" + actionName;
            string ActivityDetailsComplete = IPAddress + " " + ActivityDetails + " " + FormName;

            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("FormID-INT", "0");
                nv.Add("ActivityID-INT", ActivityID.ToString());
                nv.Add("UserID-INT", UserID);
                nv.Add("ActivityDetails-VARCHAR", ActivityDetailsComplete);
                Result = _DAL.InsertData("sp_insert_activitylog", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                _logger.LogInformation("{0} {1} {2}", controllerName, MethodBase.GetCurrentMethod().Name, ActivityDetailsComplete);
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2} {3}", controllerName, MethodBase.GetCurrentMethod().Name, ActivityDetailsComplete, ex.Message);
            }
        }

        #endregion

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveCOBInitiateRequest([FromBody] ClientInfo client)
        {
            if (client == null)
            {
                return BadRequest("Client object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflow-VARCHAR", HttpUtility.HtmlEncode(client.workflow));
                nv.Add("instanceid-INT", client.instanceid == null ? "0" : client.instanceid.ToString());

                // Client
                nv.Add("client_name-NVARCHAR", client.client_name ?? "");
                nv.Add("industry_id-INT", client.industry_id?.ToString() ?? "0");
                nv.Add("company_size_id-INT", client.company_size_id?.ToString() ?? "0");
                nv.Add("country_id-INT", client.country_id?.ToString() ?? "0");

                nv.Add("website-NVARCHAR", client.website ?? "");
                nv.Add("billing_address-NVARCHAR", client.billing_address ?? "");
                nv.Add("tax_registration_no-NVARCHAR", client.tax_registration_no ?? "");

                // Contact
                nv.Add("primary_contact_name-NVARCHAR", client.primary_contact_name ?? "");
                nv.Add("primary_contact_email-NVARCHAR", client.primary_contact_email ?? "");
                nv.Add("primary_contact_phone-NVARCHAR", client.primary_contact_phone ?? "");

                // Commercial
                nv.Add("contract_type_id-INT", client.contract_type_id.ToString() ?? "0");
                nv.Add("account_owner_id-INT", client.account_owner_id.ToString() ?? "0");
                nv.Add("support_owner_id-INT", client.support_owner_id.ToString() ?? "0");

                // Dates (IMPORTANT FIX)
                nv.Add("onboarding_start_date-DATETIME", client.onboarding_start_date?.ToString("s"));
                nv.Add("onboarding_completion_date-DATETIME", client.onboarding_completion_date?.ToString("s"));
                nv.Add("billing_start_date-DATETIME", client.billing_start_date?.ToString("s"));
                nv.Add("contract_start_date-DATETIME", client.contract_start_date?.ToString("s"));
                nv.Add("contract_end_date-DATETIME", client.contract_end_date?.ToString("s"));

                // Flags
                nv.Add("high_value_client-BIT", client.high_value_client ? "1" : "0");
                nv.Add("priority_level-VARCHAR", client.priority_level ?? "Low");

                nv.Add("createdby-INT", client.userid.ToString());

                dt = _DAL.GetData("sp_insert_client_info", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_client_info");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_client_info");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveContractInfo([FromBody] ContractInfo client)
        {
            if (client == null)
            {
                return BadRequest("Client object is null");
            }

            bool Result = false;
            DataTable dt;
            try
            {
                client.client_id = 123;

                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                // =========================
                // COMMON
                // =========================
                nv.Add("workflow-VARCHAR", HttpUtility.HtmlEncode(client.workflow));
                nv.Add("instanceid-INT", client.instanceid?.ToString() ?? "0");
                nv.Add("client_id-INT", client.client_id?.ToString() ?? "0");

                // =========================
                // CONTRACT CORE
                // =========================
                nv.Add("basic_contract_type-INT", client.basic_contract_type?.ToString() ?? "0");
                nv.Add("contract_value-DECIMAL", client.contract_value?.ToString() ?? "0");
                nv.Add("currency_id-INT", client.currency_id?.ToString() ?? "0");

                nv.Add("discount_percent-DECIMAL", client.discount_percent?.ToString() ?? "0");
                nv.Add("tax_percent-DECIMAL", client.tax_percent?.ToString() ?? "0");

                // =========================
                // BILLING
                // =========================
                nv.Add("bill_freq_id-INT", client.bill_freq_id?.ToString() ?? "0");

                nv.Add("bill_start_date-DATETIME",
                    client.bill_start_date?.ToString("yyyy-MM-dd HH:mm:ss"));

                nv.Add("bill_end_date-DATETIME",
                    client.bill_end_date?.ToString("yyyy-MM-dd HH:mm:ss"));

                nv.Add("billing_cycle-INT", client.billing_cycle?.ToString() ?? "0");

                // =========================
                // RENEWAL
                // =========================

                int month = 0;
                if (!string.IsNullOrEmpty(client.renewal_terms_months))
                {
                    month = Convert.ToInt32(client.renewal_terms_months.Split('-')[1]);
                }

                nv.Add("renewal_terms_months-INT", month.ToString());
                nv.Add("auto_renew-BIT", client.auto_renew ? "1" : "0");

                // =========================
                // PROJECT
                // =========================
                nv.Add("project_name-NVARCHAR", HttpUtility.HtmlEncode(client.project_name));
                nv.Add("project_value-DECIMAL", client.project_value?.ToString() ?? "0");
                nv.Add("no_of_milestones-INT", client.no_of_milestones?.ToString() ?? "0");

                // =========================
                // PAYMENT / SUPPORT
                // =========================
                nv.Add("payment_term_id-INT", client.payment_term_id?.ToString() ?? "0");

                nv.Add("penalty_terms-NVARCHAR", HttpUtility.HtmlEncode(client.penalty_terms));
                nv.Add("support_hours-NVARCHAR", HttpUtility.HtmlEncode(client.support_hours));

                // =========================
                // SYSTEM
                // =========================
                nv.Add("createdby-INT", client.userid?.ToString() ?? "0");

                dt = _DAL.GetData("sp_insert_contract_info", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_contract_info");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_contract_info");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt.Rows.Count > 0)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }


        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult GetCOBDetailByInstanceId(int instanceid)
        {
            bool Result = false;
            DataTable dt;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("instanceid-INT", instanceid.ToString());

                dt = _DAL.GetData("sp_select_COBDetailByInstanceId", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_select_COBDetailByInstanceId");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_select_COBDetailByInstanceId");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (dt != null)
            {
                return Ok(dt);
            }
            else
            {
                return BadRequest(dt);
            }
        }

    }
}
