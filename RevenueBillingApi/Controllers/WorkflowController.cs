using RevenueBillingApi.Context;
using RevenueBillingApi.Extensions;
using RevenueBillingApi.Models.Settings;
using RevenueBillingApi.Models.Workflow;
using RevenueBillingApi.RateLimiting;
using RevenueBillingApi.Utility;
using RevenueBillingApp.Models.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Web;

namespace RevenueBillingApi.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/{controller}/{action}/{id:int?}")]
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private readonly DataAccessLayer _DAL;
        private readonly SendEmail _sendemail;
        private readonly ILogger<SettingsController> _logger;
        private readonly DataEncryptor _dataencryptor;
        private readonly RandomStringGenerator _randomstringgenerator;
        public WorkflowController(DataAccessLayer DAL, ILogger<SettingsController> logger, SendEmail sendemail, DataEncryptor dataencryptor, RandomStringGenerator randomstringgenerator)
        {
            _DAL = DAL;
            _logger = logger;
            _sendemail = sendemail;
            _dataencryptor = dataencryptor;
            _randomstringgenerator = randomstringgenerator;
        }

        #region Activity Log
        public void SystemActivityLog(int? ActivityID, string? ActivityDetails)
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

        #region Workflow
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWorkflow()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_workflow", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_workflow");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_workflow");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWorkflowByID(string workflowid)
        {
            DataTable dt = new DataTable();
            string methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("id-INT", workflowid);
                dt = _DAL.GetData("sp_select_workflow_byid", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_workflow_byid");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_workflow_byid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult EditWorkflow([FromBody] Workflow workflow)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("id-INT", workflow.id.ToString());
                nv.Add("workflowname-VARCHAR", HttpUtility.HtmlEncode(workflow.workflowname));
                nv.Add("isactive-BIT", workflow.isactive == true ? "1" : "0");
                nv.Add("editby-INT", workflow.editdby);
                nv.Add("workflowcode-VARCHAR", HttpUtility.HtmlEncode(workflow.workflowcode));
                nv.Add("mainformurl-VARCHAR", HttpUtility.HtmlEncode(workflow.formpageurl));
                nv.Add("viewpageurl-VARCHAR", HttpUtility.HtmlEncode(workflow.viewpageurl));
                nv.Add("mytaskpageurl-VARCHAR", HttpUtility.HtmlEncode(workflow.taskpageurl));
                nv.Add("myrequestpageurl-VARCHAR", HttpUtility.HtmlEncode(workflow.requestpageurl));

                Result = _DAL.InsertData("sp_update_workflow", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update + "sp_update_workflow");

                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Update, ActivityLog.ActivityDetails_Update2 + "sp_update_workflow");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveWorkflow([FromBody] Workflow workflow)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflowname-VARCHAR", HttpUtility.HtmlEncode(workflow.workflowname));
                nv.Add("isactive-BIT", workflow.isactive == true ? "1" : "0");
                nv.Add("createdby-INT", workflow.createdby);
                nv.Add("workflowcode-VARCHAR", workflow.workflowcode.ToString());
                nv.Add("mainformurl-VARCHAR", workflow.formpageurl.ToString());
                nv.Add("viewpageurl-VARCHAR", workflow.viewpageurl.ToString());
                nv.Add("mytaskpageurl-VARCHAR", workflow.taskpageurl.ToString());
                nv.Add("myrequestpageurl-VARCHAR", workflow.requestpageurl.ToString());
                Result = _DAL.InsertData("sp_insert_workflow", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_workflow");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_workflow");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteWorkflow([FromBody] Step step)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("ID-INT", step.id?.ToString());
                nv.Add("editby-INT", step.editdby?.ToString());
                Result = _DAL.InsertData("sp_delete_workflow", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_delete_workflow");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_delete_workflow");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }
        #endregion

        #region WorkflowStep

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetApprovalType()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_approvaltype", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_approvaltype");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_approvaltype");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveWorkflowStep([FromBody] Step step)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflowid-INT", step.workflowid?.ToString());
                nv.Add("workflowstep-VARCHAR", HttpUtility.HtmlEncode(step.workflowstep));
                nv.Add("RoleID-INT", step.RoleID == null ? "NULL" : step.RoleID.ToString());
                nv.Add("sla-INT", step.sla?.ToString());
                nv.Add("sortid-INT", step.sortid?.ToString());
                nv.Add("approvaltypeid-INT", step.approvaltypeid?.ToString());
                nv.Add("isactive-BIT", step.isactive == true ? "1" : "0");
                nv.Add("createdby-INT", step.createdby?.ToString());

                Result = _DAL.InsertData("sp_insert_workflowstep", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_workflowstep");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_workflowstep");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult UpdateWorkflowStep([FromBody] Step step)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("ID-INT", step.id?.ToString());
                nv.Add("workflowid-INT", step.workflowid?.ToString());
                nv.Add("workflowstep-VARCHAR", HttpUtility.HtmlEncode(step.workflowstep));
                nv.Add("RoleID-INT", step.RoleID == null ? "NULL" : step.RoleID.ToString());
                nv.Add("sla-INT", step.sla?.ToString());
                nv.Add("sortid-INT", step.sortid?.ToString());
                nv.Add("approvaltypeid-INT", step.approvaltypeid?.ToString());
                nv.Add("isactive-BIT", step.isactive == true ? "1" : "0");
                nv.Add("editdby-INT", step.editdby?.ToString());

                Result = _DAL.InsertData("sp_update_workflowstep", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_update_workflowstep");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_update_workflowstep");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteWorkflowStep([FromBody] Step step)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("ID-INT", step.id?.ToString());
                nv.Add("editby-INT", step.editdby?.ToString());
                Result = _DAL.InsertData("sp_delete_workflowstep", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_delete_workflowstep");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_delete_workflowstep");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWorkflowStep()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_workflowstep", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_workflowstep");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_workflowstep");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWorkflowStepsByID(string workflowstepid)
        {
            DataTable dt = new DataTable();
            string methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("id-INT", workflowstepid);
                dt = _DAL.GetData("sp_select_workflowstepbyid", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_workflow_byid");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_workflow_byid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetSectionPermission(int workflowid, int stepid)
        {
            DataTable dt = new DataTable();
            string methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("workflowid-INT", workflowid.ToString());
                nv.Add("stepid-INT", stepid.ToString());
                dt = _DAL.GetData("sp_select_SectionPermission", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_SectionPermission");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_SectionPermission");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult PerformVisibleTask([FromBody] SectionPermission section)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("sectionpermissionid-INT", section.sectionpermissionid.ToString());
                nv.Add("isvisible-BIT", section.isvisible.ToString());
                nv.Add("editedby-INT", section.editby?.ToString());
                Result = _DAL.InsertData("sp_update_sectionpermissionvisibletask", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_update_sectionpermissionvisibletask");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_update_sectionpermissionvisibletask");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult PerformEnableTask([FromBody] SectionPermission section)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("sectionpermissionid-INT", section.sectionpermissionid.ToString());
                nv.Add("isenable-BIT", section.isenable.ToString());
                nv.Add("editedby-INT", section.editby?.ToString());
                Result = _DAL.InsertData("sp_update_sectionpermissionenabletask", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_update_sectionpermissionenabletask");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_update_sectionpermissionenabletask");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }
        #endregion

        #region WorkFlowStepAction

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveWorkflowStepAction([FromBody] Models.Workflow.Action action)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("actionname-VARCHAR", HttpUtility.HtmlEncode(action.actionname));
                nv.Add("stepid-INT", action.workflowstepid?.ToString());
                nv.Add("nextstepid-INT", action.workflownextstepid?.ToString());
                nv.Add("isactive-BIT", action.isactive == true ? "1" : "0");
                nv.Add("createdby-INT", action.createdby?.ToString());
                nv.Add("ismove-BIT", action.ismove == true ? "1" : "0");
                nv.Add("issave-BIT", action.issave == true ? "1" : "0");
                nv.Add("workflowid-INT", action.workflowcode.ToString());
                nv.Add("assignmenttype-VARCHAR", HttpUtility.HtmlEncode(action.assignmentType));
                nv.Add("dynamicfunction-VARCHAR", action.nexttype == null ? "NULL" : HttpUtility.HtmlEncode(action.nexttype));
                nv.Add("actiontype-VARCHAR", HttpUtility.HtmlEncode(action.actiontype));

                Result = _DAL.InsertData("sp_insert_workflowstepaction", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_workflowstepaction");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_workflowstepaction");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult UpdateWorkflowStepAction([FromBody] Models.Workflow.Action action)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("ID-INT", action.id?.ToString());
                nv.Add("actionname-VARCHAR", HttpUtility.HtmlEncode(action.actionname));
                nv.Add("stepid-INT", action.workflowstepid?.ToString());
                nv.Add("nextstepid-INT", action.workflownextstepid?.ToString());
                nv.Add("isactive-BIT", action.isactive == true ? "1" : "0");
                nv.Add("editdby-INT", action.editdby?.ToString());
                nv.Add("ismove-BIT", action.isactive == true ? "1" : "0");
                nv.Add("issave-BIT", action.isactive == true ? "1" : "0");
                nv.Add("workflowid-INT", action.workflowcode.ToString());
                nv.Add("assignmenttype-VARCHAR", HttpUtility.HtmlEncode(action.assignmentType));
                nv.Add("dynamicfunction-VARCHAR", action.nexttype == null ? "NULL" : HttpUtility.HtmlEncode(action.nexttype));
                nv.Add("actiontype-VARCHAR", HttpUtility.HtmlEncode(action.actiontype));
                Result = _DAL.InsertData("sp_update_workflowstepaction", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_update_workflowstepaction");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_update_workflowstepaction");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetDynamicFunction()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_dynamicfunction", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_dynamicfunction");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_dynamicfunction");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWorkflowStepbyWorkflowId(int workflowid)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("id-INT", workflowid.ToString());
                dt = _DAL.GetData("sp_select_workflowstepbyworkflowid", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_workflowstepbyworkflowid");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_workflowstepbyworkflowid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }


        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult DeleteWorkflowStepAction([FromBody] Models.Workflow.Action action)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("ID-INT", action.id?.ToString());
                nv.Add("editby-INT", action.editdby?.ToString());
                Result = _DAL.InsertData("sp_delete_workflowstepaction", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_delete_workflowstepaction");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_delete_workflowstepaction");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWorkflowStepAction()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_workflowstepaction", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_workflowstep");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_workflowstep");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWorkflowStepsActionByID(string workflowstepid)
        {
            DataTable dt = new DataTable();
            string methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("ID-INT", workflowstepid);
                dt = _DAL.GetData("sp_select_workflowstepactionbyid", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_workflowstepactionbyid");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_workflowstepactionbyid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWorkflowCode()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = _DAL.GetData("sp_select_workflowcode", null, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_workflowcode");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_workflowcode");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        #endregion

        #region Generic Workflow
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetFormsByInstanceId(string? wfcode, int instanceid,string formName)
        {
            DataTable dt = new DataTable();
            string jsonResult = "";
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("wfcode-VARCHAR", HttpUtility.HtmlEncode(wfcode));
                nv.Add("instanceid-INT", instanceid.ToString());
                nv.Add("formname-VARCHAR", formName.ToString());

                dt = _DAL.GetData("sp_select_formsection", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    jsonResult = dt.Rows[0][0]?.ToString();
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_formsection");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_formsection");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(jsonResult);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetCurrentWorkflowAction(string wfcode,int instanceid)
        {
            DataTable dt = new DataTable();
            string jsonResult = "";
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("wfcode-VARCHAR", HttpUtility.HtmlEncode(wfcode));
                nv.Add("instanceid-INT", instanceid.ToString());

                dt = _DAL.GetData("sp_select_actions", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    jsonResult = dt.Rows[0][0]?.ToString();
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_actions");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_actions");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(jsonResult);
        }
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetWorkflowLog(int instanceid)
        {
            DataTable dt = new DataTable();
            string jsonResult = "";
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("instanceid-INT", instanceid.ToString());

                dt = _DAL.GetData("sp_select_workflowhistory", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    jsonResult = dt.Rows[0][0]?.ToString();
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_workflowhistory");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_workflowhistory");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(jsonResult);
        }
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult MoveWorkflow([FromBody] WorkflowMove step)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("instanceid-INT", step.instanceid?.ToString());
                nv.Add("actionid-INT", step.actionid?.ToString());
                nv.Add("dynamicfunction-VARCHAR", step.dynamicfunction == null ? "NULL" : HttpUtility.HtmlEncode(step.dynamicfunction));
                nv.Add("assignmenttype-VARCHAR", step.assignmenttype == null ? "NULL" : HttpUtility.HtmlEncode(step.assignmenttype));
                nv.Add("userid-INT", step.userid?.ToString());
                nv.Add("comment-VARCHAR", step.comment == null ? "NULL" : HttpUtility.HtmlEncode(step.comment));

                Result = _DAL.InsertData("sp_workflowmovetonext", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_workflowmovetonext");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_workflowmovetonext");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult MyRequest(string wfcode, int roleid,int userid)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("wfcode-VARCHAR", HttpUtility.HtmlEncode(wfcode));
                nv.Add("userid-INT", userid.ToString());
                dt = _DAL.GetData("sp_select_myrequest", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_myrequest");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_myrequest");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult MyTask(string wfcode, int roleid, int userid)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("wfcode-VARCHAR", HttpUtility.HtmlEncode(wfcode));
                nv.Add("roleid-INT", roleid.ToString());
                nv.Add("userid-INT", userid.ToString());
                dt = _DAL.GetData("sp_select_mytask", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_mytask");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_mytask");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult MyApproval(string wfcode, int roleid, int userid)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("wfcode-VARCHAR", HttpUtility.HtmlEncode(wfcode));
                nv.Add("userid-INT", userid.ToString());
                dt = _DAL.GetData("sp_select_myapproval", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_myapproval");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_myapproval");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetDailyTasks(int userid,int roleid)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("userid-INT", userid.ToString());
                nv.Add("roleid-INT", roleid.ToString());
                dt = _DAL.GetData("sp_select_dailytask", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_dailytask");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_dailytask");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetDailyOverdueTasks(int userid,int roleid)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("userid-INT", userid.ToString());
                nv.Add("roleid-INT", roleid.ToString());
                dt = _DAL.GetData("sp_select_dailyoverduetask", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_dailyoverduetask");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_dailyoverduetask");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetDailyMyRequests(int userid)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("userid-INT", userid.ToString());
                dt = _DAL.GetData("sp_select_dailymyrequest", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_dailymyrequest");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_dailymyrequest");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetDailyOverdueMyRequests(int userid)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("userid-INT", userid.ToString());
                dt = _DAL.GetData("sp_select_dailymyoverduerequest", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_dailymyoverduerequest");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_dailymyoverduerequest");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }
        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetDailyMyApprovals(int userid)
        {
            DataTable dt = new DataTable();
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("userid-INT", userid.ToString());
                dt = _DAL.GetData("sp_select_dailymyapprovals", nv, _DAL.CSManagementPortalDatabase);

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_dailymyapprovals");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_dailymyapprovals");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }
        [RateLimitMiddleware(50, 5)]
        [HttpPost]
        public IActionResult SaveSelfAssignUser([FromBody] SelfAssign step)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("ordernumber-INT", step.ordernumber?.ToString());
                nv.Add("instanceid-INT", step.instanceid?.ToString());
                nv.Add("userid-INT", step.userid?.ToString());
                nv.Add("createdby-INT",step.createdby?.ToString());
                Result = _DAL.InsertData("sp_insert_selfassignuser", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_insert_selfassignuser");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_insert_selfassignuser");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }


        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult GetSelfAssignUser(int instanceid)
        {
            DataTable dt = new DataTable();
            string methodName = MethodBase.GetCurrentMethod().Name;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("instanceid-INT", instanceid.ToString());
                dt = _DAL.GetData("sp_select_selfassignuser", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_selfassignuser");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_selfassignuser");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(dt);
        }
        [RateLimitMiddleware(50, 5)]
        [HttpGet]
        public IActionResult DeleteSelfAssignUser(int primaryid, int instanceid)
        {
            bool Result = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("id-INT", primaryid.ToString());
                nv.Add("instanceid-INT", instanceid.ToString());
                Result = _DAL.InsertData("sp_delete_selfassignuser", nv, _DAL.CSManagementPortalDatabase);
                nv = null;

                if (Result)
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert + "sp_delete_selfassignuser");
                }
                else
                {
                    SystemActivityLog(ActivityLog.ActivityID_Insert, ActivityLog.ActivityDetails_Insert2 + "sp_delete_selfassignuser");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }

            if (Result)
            {
                return Ok(Result);
            }
            else
            {
                return BadRequest(Result);
            }
        }

        [RateLimitMiddleware(100, 5)]
        [HttpGet]
        public IActionResult IsTaskAllowed(int instanceid, int roleid, int userid)
        {
            DataTable dt = new DataTable();
            bool isAllowed = false;
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                nv.Add("instanceid-INT", instanceid.ToString());
                nv.Add("roleid-INT", roleid.ToString());
                nv.Add("userid-INT", userid.ToString());
                dt = _DAL.GetData("sp_select_istaskallowed", nv, _DAL.CSManagementPortalDatabase);

  
                if (dt != null && dt.Rows.Count > 0)
                {
                    // The SP returns a table with one column and one row: 1 or 0
                    var value = dt.Rows[0][0].ToString();
                    isAllowed = value == "1";
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get + "sp_select_mytask");
                }
                else
                {
                    isAllowed = false;
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "sp_select_mytask");
                }
            }
            catch (Exception ex)
            {
                isAllowed = false;
                _logger.LogError("{0} {1} {2}", "FormsController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
            return Ok(isAllowed);
        }
        #endregion

        #region File Upload New

        [HttpPost]
        [Route("UploadToFileSystem/{version}/{description}/{user_ID}/{instanceID}")]
        public IActionResult UploadToFileSystem(IList<IFormFile> files, string version, string description, int user_ID, int instanceID)
        {
            var Result = false;
            var decodeDescipt = HttpUtility.UrlDecode(description);
            try
            {
                NameValueCollection? nv = new NameValueCollection();
                nv.Clear();
                DataTable dt = new DataTable();
                foreach (var file in files)
                {
                    var basePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Files\\");
                    bool basePathExists = System.IO.Directory.Exists(basePath);
                    if (!basePathExists) Directory.CreateDirectory(basePath);
                    var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    var filePath = Path.Combine(basePath, file.FileName);
                    var extension = Path.GetExtension(file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyToAsync(stream);
                    }

                    nv.Add("instanceid-INT", instanceID.ToString());
                    nv.Add("name-VARCHAR", fileName.ToString());
                    nv.Add("extension-VARCHAR", extension.ToString());
                    nv.Add("version-VARCHAR", version.ToString());
                    nv.Add("description-VARCHAR", description.ToString());
                    nv.Add("uploadedBy-VARCHAR", user_ID.ToString());
                    dt = _DAL.GetData("proc_add_filseupload", nv, _DAL.CSManagementPortalDatabase);
                }
                if (Result)
                {
                    _logger.LogInformation("");
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "proc_add_filseupload");
                    return Ok("File Save Successfully");
                }
                else
                {
                    _logger.LogError("{0} {1} {2}", "WorkflowController", MethodBase.GetCurrentMethod().Name,"");
                    SystemActivityLog(ActivityLog.ActivityID_Get, ActivityLog.ActivityDetails_Get2 + "proc_add_filseupload");
                    return BadRequest("Something Went Wrong! File Not Saved");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1} {2}", "WorkflowController", MethodBase.GetCurrentMethod().Name, ex.Message);
                SystemActivityLog(ActivityLog.ActivityID_Error, MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                return BadRequest("Something Went Wrong Please Contact Your Sysmtem Adminsitrator");
            }
        }

        #endregion


    }
}
