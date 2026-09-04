using RevenueBillingApp.Models.Base;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using RevenueBillingApp.Utility;
using System.Data;
using RevenueBillingApp.Models.Authentication;
using RevenueBillingApp.Models.Session;
using Microsoft.AspNetCore.Authorization;
using RevenueBillingApp.Extensions;
using RevenueBillingApp.Models.FileUpload;
using System.Reflection;
using RevenueBillingApi.Models.Workflow;
using RevenueBillingApp.Middleware;
using System.Security.Cryptography;
using RevenueBillingApp.Models.BillingRevenue;
using System.Linq;

namespace RevenueBillingApp.Controllers
{
    [Authorize(AuthenticationSchemes = "ASPXAUTH")]
    public class BaseController : Controller
    {
        private readonly Sessions _sessions;
        private readonly RequestClient _requestClient;
        private readonly ILogger<BaseController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly DataEncryptor _dataEncryptor;

        public BaseController(RequestClient requestClient, IConfiguration config, ILogger<BaseController> logger, DataEncryptor dataEncryptor, IWebHostEnvironment environment)
        {
            _requestClient = requestClient;
            _logger = logger;
            _dataEncryptor = dataEncryptor;
            _environment = environment;
        }

        #region Session
        [HttpGet]
        public List<RolesMapping> GetSessionForHtml(string RouteValues)
        {
            List<RolesMapping> tlistFiltered = new List<RolesMapping>();

            try
            {
                tlistFiltered = _requestClient.GetSessionForHtml(RouteValues);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("BaseController/GetSession {1}", ex.Message);
            }

            return tlistFiltered;
        }
        #endregion
      
        #region Settings

        #region UserMaster

        //[HttpGet]
        //[TypeFilter(typeof(AllowedApiAccess))]
        //public IActionResult GetUserType()
        //{
        //    UserInfo _info = _requestClient.GetUserInformation();

        //    HttpResponseMessage response = _requestClient.UseHttpClientGet("?TypeID=" + _info.UserTypeID, "GetUserType", "Settings");
        //    return new HttpResponseMessageResult(response);
        //}

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetUsers()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetUsersByType", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetForms()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetForms", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetUserRoles()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetRoles", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetMapping(string RoleId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?RoleId=" + RoleId, "GetMapping", "Settings");
            return new HttpResponseMessageResult(response);
        }


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult FetchAdUsers()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "FetchAdUsers", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditUsers([FromForm] User Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();

            Data.UserTypeID = "3";
            Data.EditUserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditUsers", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveUsers([FromForm] User Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserTypeID = "3";
            Data.EditUserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveUsers", "Settings");
            
            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteUsers([FromBody] DeleteFromDB Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditUserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteUsers", "Settings");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region RoleMaster

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetRoles()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetRoles", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetRolesByID(string RoleID)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?RoleID=" + RoleID, "GetRolesByID", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditRoles([FromForm] Role Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.UserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditRoles", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveRoles([FromForm] Role Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.UserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveRoles", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteRoles([FromBody] DeleteFromDB Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.UserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteRoles", "Settings");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region SystemPriviliges

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetMapping_2(string RoleId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?RoleId=" + RoleId, "GetMapping_2", "Settings");
            return new HttpResponseMessageResult(response);
        }
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult UpdateRolesMapping([FromBody] RoleMapping Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.UserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "UpdateRolesMapping", "Settings");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region SmtpSetting
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SmtpSettingUpdate([FromBody] SMTPSettings smtpSetting)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            
            string encryptpassword = _dataEncryptor.EncryptPassword(smtpSetting.SmtpPassword);
            smtpSetting.SmtpPassword = encryptpassword;
            string jsonString = JsonConvert.SerializeObject(smtpSetting);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SmtpSettingUpdate", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SmtpSettingGet()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "SmtpSettingGet", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetSmtpGrid()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetSmtpGrid", "Settings");
            return new HttpResponseMessageResult(response);
        }


        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SetDefaultSMTP([FromForm] SMTPDefaultModel model)
        {
            string jsonString = JsonConvert.SerializeObject(model);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SetDefaultSMTP", "Settings");
            return new HttpResponseMessageResult(response);
        }


        #endregion

        #region UserProfile
        [TypeFilter(typeof(AllowedExtensionsAttribute))]
        //[TypeFilter(typeof(MaxFileSizeAttribute))]
        [TypeFilter(typeof(AllowedApiAccess))]
        [HttpPost]
        public IActionResult UpdateProfile([FromForm] UserProfile profile)
        {


            string fname_partial = null;
            string wwwPath = this._environment.WebRootPath;
            string contentPath = this._environment.ContentRootPath;


            if (profile._ImageFile != null)
            {
                string fname = null;

                if (Request.Headers["User-Agent"].ToString().ToUpper() == "IE" || Request.Headers["User-Agent"].ToString().ToUpper() == "INTERNETEXPLORER")
                {
                    string[] testfiles = profile._ImageFile.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }
                else
                {
                    fname = profile._ImageFile.FileName;
                }

                string extension = Path.GetExtension(profile._ImageFile.FileName);

                fname_partial = DateTime.Now.Millisecond.ToString() + "_" + extension;

                fname = Path.Combine(this._environment.WebRootPath + "\\Public\\image\\UserImage", fname_partial);

                using (FileStream stream = new FileStream(fname, FileMode.Create))
                {
                    profile._ImageFile.CopyTo(stream);
                    _logger.LogInformation("{0} {1} {2}", MethodBase.GetCurrentMethod().Name, fname, "FileSaved");
                }
            }

            profile.ImageFile = fname_partial;

            string jsonString = JsonConvert.SerializeObject(profile);

            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "UpdateProfile", "Settings");

            return new HttpResponseMessageResult(response);

        }

        #endregion

        #region Organization Chart

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetAllUsers()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetAllUsers", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveOrganizationChart([FromForm] OrganizationChart Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.createdby = userinfo.UserID;
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveOrganizationChart", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetOrganizationChart()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetOrganizationChart", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetEmployeeListByManagerID(int managerid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?managerid=" + managerid, "GetEmployeeListByManagerID", "Settings");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetOrganizationHierarchy()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetOrganizationHierarchy", "Settings");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region Get Cities
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetAllCities()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetAllCities", "Settings");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #endregion

        #region Instrument Data
        
        #region Instrument
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetInstruments()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?TypeID=" + _info.UserTypeID, "GetInstruments", "Data");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetInstrumentTypes()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?TypeID=" + _info.UserTypeID, "GetInstrumentTypes", "Data");
            return new HttpResponseMessageResult(response);
        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetTenures()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?TypeID=" + _info.UserTypeID, "GetTenures", "Data");
            return new HttpResponseMessageResult(response);
        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetFrequency()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?TypeID=" + _info.UserTypeID, "Frequency", "Data");
            return new HttpResponseMessageResult(response);
        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetBenchmark()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?TypeID=" + _info.UserTypeID, "GetBenchmark", "Data");
            return new HttpResponseMessageResult(response);
        }
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditInstrument([FromForm] Role Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.UserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditInstrument", "Data");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveInstrument([FromForm] Role Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.UserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveInstrument", "Data");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteInstrument([FromBody] DeleteFromDB Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.UserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteInstrument", "Data");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #endregion

        #region Main Workflow Common

        #region WorkflowMethod


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWorkflow()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetWorkflow", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWorkflowByID(string workflowid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?workflowid=" + workflowid, "GetWorkflowByID", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditWorkflow([FromForm] Workflow workflow)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            workflow.editdby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(workflow);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditWorkflow", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveWorkflow([FromForm] Workflow workflow)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            workflow.createdby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(workflow);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveWorkflow", "Workflow");
            return new HttpResponseMessageResult(response);
        }



        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteWorkflow([FromBody] Step step)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            step.editdby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(step);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteWorkflow", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult MyRequest(string wfcode)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?wfcode=" + wfcode + "&roleid=" + userinfo.RoleID + "&userid=" + userinfo.UserID, "MyRequest", "Workflow");
            return new HttpResponseMessageResult(response);

        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult MyApproval(string wfcode)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?wfcode=" + wfcode + "&roleid=" + userinfo.RoleID + "&userid=" + userinfo.UserID, "MyApproval", "Workflow");
            return new HttpResponseMessageResult(response);

        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult MyTask(string wfcode)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?wfcode=" + wfcode + "&roleid=" + userinfo.RoleID + "&userid=" + userinfo.UserID, "MyTask", "Workflow");
            return new HttpResponseMessageResult(response);

        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult IsTaskAllowed(int instanceid)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?instanceid=" + instanceid + "&roleid=" + userinfo.RoleID + "&userid=" + userinfo.UserID, "IsTaskAllowed", "Workflow");
            return new HttpResponseMessageResult(response);

        }

    #endregion

        #region  WorkflowStep

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetApprovalType()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetApprovalType", "Workflow");
            return new HttpResponseMessageResult(response);
        }


        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveWorkflowStep([FromForm] Step step)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            step.createdby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(step);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveWorkflowStep", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult UpdateWorkflowStep([FromForm] Step step)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            step.editdby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(step);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "UpdateWorkflowStep", "Workflow");
            return new HttpResponseMessageResult(response);
        }


        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteWorkflowStep([FromBody] Step step)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            step.editdby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(step);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteWorkflowStep", "Workflow");
            return new HttpResponseMessageResult(response);
        }


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWorkflowStep()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetWorkflowStep", "Workflow");
            return new HttpResponseMessageResult(response);
        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWorkflowStepsByID(string workflowstepid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?workflowstepid=" + workflowstepid, "GetWorkflowStepsByID", "Workflow");
            return new HttpResponseMessageResult(response);
        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetSectionPermission(int workflowid, int stepid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet($"?workflowid={workflowid}&stepid={stepid}", "GetSectionPermission", "Workflow");
            return new HttpResponseMessageResult(response);
        }
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult PerformVisibleTask([FromBody] SectionPermission section)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            section.editby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(section);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "PerformVisibleTask", "Workflow");
            return new HttpResponseMessageResult(response);
        }
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult PerformEnableTask([FromBody] SectionPermission section)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            section.editby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(section);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "PerformEnableTask", "Workflow");
            return new HttpResponseMessageResult(response);
        }


        #endregion

        #region WorkFlowStepAction

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveWorkflowStepAction([FromForm] Models.Base.Action action)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            action.createdby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(action);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveWorkflowStepAction", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult UpdateWorkflowStepAction([FromForm] Models.Base.Action action)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            action.editdby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(action);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "UpdateWorkflowStepAction", "Workflow");
            return new HttpResponseMessageResult(response);
        }


        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteWorkflowStepAction([FromBody] Models.Base.Action action)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            action.editdby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(action);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteWorkflowStepAction", "Workflow");
            return new HttpResponseMessageResult(response);
        }


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWorkflowStepAction()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetWorkflowStepAction", "Workflow");
            return new HttpResponseMessageResult(response);
        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWorkflowStepsActionByID(string workflowstepid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?workflowstepid=" + workflowstepid, "GetWorkflowStepsActionByID", "Workflow");
            return new HttpResponseMessageResult(response);
        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWorkflowCode()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetWorkflowCode", "Workflow");
            return new HttpResponseMessageResult(response);
        }


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDynamicFunction()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetDynamicFunction", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWorkflowStepbyWorkflowId(int workflowid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?workflowid=" + workflowid, "GetWorkflowStepbyWorkflowId", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region Generic Workflow
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetFormsByInstanceId(string wfcode, int instanceid, string formName)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?wfcode=" + wfcode + "&instanceid=" + instanceid + "&formName=" + formName, "GetFormsByInstanceId", "Workflow");
            return new HttpResponseMessageResult(response);
        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCurrentWorkflowAction(string wfcode, int instanceid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?wfcode=" + wfcode + "&instanceid=" + instanceid, "GetCurrentWorkflowAction", "Workflow");
            return new HttpResponseMessageResult(response);
        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWorkflowLog(int instanceid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?instanceid=" + instanceid, "GetWorkflowLog", "Workflow");
            return new HttpResponseMessageResult(response);
        }
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult MoveWorkflow([FromForm] WorkflowMove move)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            move.userid = userinfo.UserID;
            string jsonString = JsonConvert.SerializeObject(move);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "MoveWorkflow", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveSelfAssignUser([FromForm] SelfAssign selfAssign)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            selfAssign.createdby = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(selfAssign);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveSelfAssignUser", "Workflow");
            return new HttpResponseMessageResult(response);
        }     
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetSelfAssignUser(int instanceid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?instanceid=" + instanceid, "GetSelfAssignUser", "Workflow");
            return new HttpResponseMessageResult(response);
        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteSelfAssignUser(int primaryid, int instanceid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?primaryid=" + primaryid + "&instanceid=" + instanceid, "DeleteSelfAssignUser", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region TaskDashboard
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDailyTasks()
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?userid=" + userinfo.UserID, "GetDailyTasks", "Workflow");
            return new HttpResponseMessageResult(response);

        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDailyOverdueTasks()
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?userid=" + userinfo.UserID, "GetDailyOverdueTasks", "Workflow");
            return new HttpResponseMessageResult(response);

        }
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDailyMyRequests()
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?userid=" + userinfo.UserID, "GetDailyMyRequests", "Workflow");
            return new HttpResponseMessageResult(response);

        }      
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDailyOverdueMyRequests()
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?userid=" + userinfo.UserID, "GetDailyOverdueMyRequests", "Workflow");
            return new HttpResponseMessageResult(response);

        }        
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDailyMyApprovals()
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?userid=" + userinfo.UserID, "GetDailyMyApprovals", "Workflow");
            return new HttpResponseMessageResult(response);

        }
        #endregion

        #endregion

        #region Molti-DMS

        #region KeyFactors
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetKeyFactors()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetKeyFactors", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveKeyFactors([FromBody] KeyFactors Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveKeyFactors", "DMS");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditKeyFactors([FromBody] KeyFactors Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditKeyFactors", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteKeyFactors([FromBody] KeyFactors Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteKeyFactors", "DMS");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region KPISurveyForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetAllKeyFactors()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetAllKeyFactors", "DMS");
            return new HttpResponseMessageResult(response);
        }


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetKPISurveyForm()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetKPISurveyForm", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveKPISurveyForm([FromBody] KPISurveyForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveKPISurveyForm", "DMS");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditKPISurveyForm([FromBody] KPISurveyForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditKPISurveyForm", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteKPISurveyForm([FromBody] KPISurveyForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteKPISurveyForm", "DMS");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region DealerUploader

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult LoadZone_DDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "LoadZone_DDL", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DownloadExcel([FromBody] DownloadParam model)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            // Forward request to API service (e.g., DMS)
            HttpResponseMessage response = _requestClient.UseHttpClientPost(null, "DownloadExcel", "DMS");

            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult UploadExcel([FromForm] UploadParam model, IFormFile file)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            // Build form data for forwarding file upload
            var formData = new MultipartFormDataContent();

            if (file != null)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                formData.Add(fileContent, "file", file.FileName);
            }

            // Add other model properties
            formData.Add(new StringContent(model.Monthd.ToString()), "Monthd");
            formData.Add(new StringContent(model.Year.ToString()), "Year");
            formData.Add(new StringContent(model.Zone.ToString()), "Zone");
            formData.Add(new StringContent(model.type ?? "U"), "type");
            formData.Add(new StringContent(model.ShowExcel.ToString()), "ShowExcel");
            formData.Add(new StringContent(model.ShowLogs.ToString()), "ShowLogs");

            HttpResponseMessage response = _requestClient.UseHttpClientPost(null, "UploadExcel", "DMS");

            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region MasterDealer

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDealerTypes_MasterDealer()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetDealerTypes", "DMS");
            return new HttpResponseMessageResult(response);
        }


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetZones()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetZones", "DMS");
            return new HttpResponseMessageResult(response);
        }


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetUserNames(string zoneid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?zoneid=" + zoneid, "GetUserNames", "DMS");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region KPIQuestionnaireForm

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetKPIFoms()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetKPIFoms", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetKPIQuestion(string questionid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?questionid=" + questionid, "GetKPIQuestion", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDealerTypes()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetDealerTypes", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetQuestionTypes()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetQuestionTypes", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetAnswerTypes()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetAnswerTypes", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult InsertKPIForm([FromBody] KPIForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "InsertKPIForm", "DMS");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult UpdateKPIForm([FromBody] KPIForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "UpdateKPIForm", "DMS");

            return new HttpResponseMessageResult(response);

        }

        #endregion

        #region OSForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetOSFoms()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetOSFoms", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetOSBoard(string boardid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?boardid=" + boardid, "GetOSBoard", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDealerTypes_OSFORM()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetDealerTypes", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCategories()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetCategories", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult InsertOSForm([FromBody] OSForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "InsertOSForm", "DMS");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult UpdateOSForm([FromBody] OSForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "UpdateOSForm", "DMS");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        public IActionResult UploadCustomerDocuments(CustomerDocument doc)
        {
            try
            {
                string? fname_partial = null;
                string wwwPath = this._environment.WebRootPath;
                string contentPath = this._environment.ContentRootPath;

                ClaimsPrincipal claimsPrincipal = HttpContext.User;
                var UserID = (from c in claimsPrincipal.Claims where c.Type == "UserID" select c.Value).FirstOrDefault();
                var UniqueKey = (from c in claimsPrincipal.Claims where c.Type == "Guid" select c.Value).FirstOrDefault();

                List<string> uploadedFiles = new List<string>();

                if (_sessions.SessionExist(UniqueKey, UserID))
                {
                    SessionItems sessionItems = _sessions.GetSession(UniqueKey, UserID);

                    if (doc.files != null && doc.files.Count > 0)
                    {
                        foreach (var file in doc.files)
                        {
                            string fname = null;

                            if (Request.Headers["User-Agent"].ToString().ToUpper().Contains("IE") ||
                                Request.Headers["User-Agent"].ToString().ToUpper().Contains("INTERNETEXPLORER"))
                            {
                                string[] testfiles = file.FileName.Split(new char[] { '\\' });
                                fname = testfiles[testfiles.Length - 1];
                            }
                            else
                            {
                                fname = file.FileName;
                            }

                            fname_partial = DateTime.Now.Millisecond.ToString() + "_" + fname;
                            fname = Path.Combine(this._environment.WebRootPath + "\\uploadedfiles\\", fname_partial);

                            using (FileStream stream = new FileStream(fname, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            uploadedFiles.Add(@"/uploadedfiles/" + fname_partial);
                        }
                    }
                }

                return Ok(uploadedFiles);
            }
            catch (Exception ex)
            {
                return BadRequest("Exception: " + ex.Message);
            }
        }


        #endregion

        #region CWMForm

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCWMFoms()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetCWMFoms", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCWMModel(string modelid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?modelid=" + modelid, "GetCWMModel", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCategories_CWM()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetCategories", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult UploadImage([FromBody] CWMForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "UploadImage", "DMS");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult InsertCWMForm([FromBody] CWMForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "InsertCWMForm", "DMS");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult UpdateCWMForm([FromBody] CWMForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "UpdateCWMForm", "DMS");

            return new HttpResponseMessageResult(response);

        }


        #endregion

        #region DFQForm

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDFQFoms()
        {
            UserInfo _info = _requestClient.GetUserInformation();
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetDFQFoms", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetDFQQuestion(string questionid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?questionid=" + questionid, "GetDFQQuestion", "DMS");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult InsertDFQForm([FromBody] DFQForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "InsertDFQForm", "DMS");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult UpdateDFQForm([FromBody] DFQForm Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.UserID = Convert.ToInt32(userinfo.UserID.ToString());
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "UpdateDFQForm", "DMS");

            return new HttpResponseMessageResult(response);

        }

        #endregion


        #endregion


        //Workflow . 1
        #region Budget

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveInitiateRequest([FromForm] Budget Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.userid = userinfo.UserID;
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveInitiateRequest", "Budget");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetBudgetDetailByInstanceId(int instanceid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?instanceid=" + instanceid, "GetBudgetDetailByInstanceId", "Budget");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        //Workflow . 2
        #region ClientOnboarding_COB

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveCOBInitiateRequest([FromForm] ClientInfo Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.userid = userinfo.UserID;
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveCOBInitiateRequest", "COB");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveContractInfo([FromForm] ContractInfo Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.userid = userinfo.UserID;
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveContractInfo", "COB");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCOBDetailByInstanceId(int instanceid)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?instanceid=" + instanceid, "GetCOBDetailByInstanceId", "COB");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult UploadToFileSystem([FromForm] User Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditUserID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "UploadToFileSystem", "Workflow");
            return new HttpResponseMessageResult(response);
        }

        #endregion


        //Ali Kamal ERP Module ....

        #region Master Setup

        #region Currency
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCurrency()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetCurrency", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveCurrency([FromBody] CurrencyMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveCurrency", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditCurrency([FromBody] CurrencyMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditCurrency", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteCurrency([FromBody] CurrencyMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteCurrency", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCurrencyDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetCurrencyDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }


        #endregion

        #region ConversionRate
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetConversionRate()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetConversionRate", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveConversionRate([FromBody] ConversionRate Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveConversionRate", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditConversionRate([FromBody] ConversionRate Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditConversionRate", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteConversionRate([FromBody] ConversionRate Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteConversionRate", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }


        #endregion

        #region Voucher Type
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetVoucherType()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetVoucherType", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }


        #endregion

        #region JobOrder
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetJobOrder()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetJobOrder", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveJobOrder([FromBody] JobOrder Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveJobOrder", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditJobOrder([FromBody] JobOrder Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditJobOrder", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteJobOrder([FromBody] JobOrder Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteJobOrder", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region Chart of Account

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetFillFinancialCategoryDDL()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetFillFinancialCategoryDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetAutoGLCode(string Level, string FC)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?Level=" + Level + "&FC=" + FC, "GetAutoGLCode", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetFillDropdown(string Level, string FC)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?Level=" + Level + "&FC=" + FC, "GetFillDropdown", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetFillCategory(string accountid)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?accountid=" + accountid, "GetFillCategory", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveDataLevel2([FromBody] tbl_Account_Level2 Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveDataLevel2", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveDataLevel3([FromBody] tbl_Account_Level3 Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveDataLevel3", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }


        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveDataLevel4([FromBody] tbl_Account_Level4 Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveDataLevel4", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetChartofAccount()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetChartofAccount", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }


        #endregion

        #region WarehouseMaster
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWarehouse()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetWarehouse", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveWarehouse([FromBody] WarehouseMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveWarehouse", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditWarehouse([FromBody] WarehouseMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditWarehouse", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteWarehouse([FromBody] WarehouseMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteWarehouse", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region Manufacturer
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetManufacturer()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetManufacturer", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveManufacturer([FromBody] ManufacturerMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveManufacturer", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditManufacturer([FromBody] ManufacturerMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditManufacturer", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteManufacturer([FromBody] ManufacturerMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteManufacturer", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region Product
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetProduct()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetProduct", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveProduct([FromBody] ProductMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveProduct", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditProduct([FromBody] ProductMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditProduct", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteProduct([FromBody] ProductMaster Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteProduct", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetPartyDDL(string CategoryCode)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?CategoryCode=" + CategoryCode, "GetPartyDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetWarehouseDDL()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetWarehouseDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }


        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetProductDDL()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetProductDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }







        #endregion

        #region Party / Customer or Supplier
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetParty(string CategoryCode)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?CategoryCode=" + CategoryCode, "GetParty", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveParty([FromBody] Party Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveParty", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditParty([FromBody] Party Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditParty", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteParty([FromBody] Party Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteParty", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region Get all Parent Company
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCompanyDDL()
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetCompanyDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #endregion

        #region Transaction

        #region GetVoucherTypeDDL
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetVoucherTypeDDL(string nature)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?Nature=" + nature, "GetVoucherTypeDDL", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region GetMaxVoucherID
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetMaxVoucherID(string vtype)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?VType=" + vtype, "GetMaxVoucherID", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region GetHeadDDL
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetHeadDDL(string cb, string headType)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?CB=" + cb + "&HeadType=" + headType , "GetHeadDDL", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region GetJobDDL
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetJobDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetJobDDL", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion


        // Voucher Save and Get Voucher Grid

        // Account module 
        #region Get Voucher Grid
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetVoucherMaster(string Nature)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?Nature=" + Nature, "GetVoucherMaster", "Trans");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region Save Voucher
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveVoucher([FromBody] tbl_Voucher Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveVoucher", "Trans");

            return new HttpResponseMessageResult(response);

        }
        #endregion

        #region Delete Voucher
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteVoucher([FromBody] tbl_Voucher Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteVoucher", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region Edit Voucher
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditVoucher([FromBody] tbl_Voucher Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();

            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditVoucher", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion


        // Purchase / Payable Module ...
        #region Save / Update Purchase Order
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SavePurchaseOrder([FromBody] Purchase_Order data)
        {
            UserInfo user = _requestClient.GetUserInformation();

            data.CreatedBy = data.Id > 0 ? data.CreatedBy : user.UserID;
            data.UpdatedBy = user.UserID;

            string json = JsonConvert.SerializeObject(data);

            HttpResponseMessage response =
                _requestClient.UseHttpClientPost(json, "SavePurchaseOrder", "Trans");

            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region Get Purchase Order
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetPurchaseOrder()
        {
            HttpResponseMessage response =
                _requestClient.UseHttpClientGet("", "GetPurchaseOrder", "Trans");

            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region Delete Purchase Order
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeletePurchaseOrder([FromBody] Purchase_Order data)
        {
            UserInfo user = _requestClient.GetUserInformation();
            data.UpdatedBy = user.UserID;

            string json = JsonConvert.SerializeObject(data);

            HttpResponseMessage response =
                _requestClient.UseHttpClientPost(json, "DeletePurchaseOrder", "Trans");

            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region GetMaxID
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetMaxID(string table)
        {
            HttpResponseMessage response =
                _requestClient.UseHttpClientGet("?table=" + table, "GetMaxID", "Trans");

            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region GetPurchaseOrderById
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetPurchaseOrderById(int POId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?POId=" + POId, "GetPurchaseOrderById", "Trans");

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Something went wrong in TransController");
            }

            var content = response.Content.ReadAsStringAsync().Result;

            // Deserialize the response to match frontend expectation
            var json = JsonConvert.DeserializeObject<dynamic>(content);

            return Ok(new
            {
                Master = json?.Master ?? json?.Tables?[0]?.Rows,
                Detail = json?.Detail ?? json?.Tables?[1]?.Rows
            });
        }

        #endregion


        #endregion

        #region Reports


        #endregion


        // Revenue & Billing Suite ... (Separate Forms Save)
        #region Revenue & Billing Suite (Separate Forms Save)

        #region Client Setup
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetClient()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetClient", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveClient([FromBody] Client Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);

            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveClient", "MasterSetup");

            return new HttpResponseMessageResult(response);

        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EditClient([FromBody] Client Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "EditClient", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteClient([FromBody] Client Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteClient", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region Populate DDLs
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetClientDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetClientDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetIndustryDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetIndustryDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCompanySizeDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetCompanySizeDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetCountryDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetCountryDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetContractTypeDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetContractTypeDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetAccountOwnerDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetAccountOwnerDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetSupportOwnerDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetSupportOwnerDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetContractTypeBasicDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetContractTypeBasicDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetBillingFrequencyDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetBillingFrequencyDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetPaymentTermsDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetPaymentTermsDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetBillingTypeDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetBillingTypeDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetBillingStatusDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetBillingStatusDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetClientInfoDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetClientInfoDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetContractInfoDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetContractInfoDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetRecurringBillingDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetRecurringBillingDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetMilestoneDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetMilestoneDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetInvoiceDDL()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "GetInvoiceDDL", "MasterSetup");
            return new HttpResponseMessageResult(response);
        }


        #endregion

        // Transactions All Forms ....
        #region SaveClientonBoarding
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        [DisableRequestSizeLimit]
        public IActionResult SaveClientonBoarding([FromForm] ClientForm Data, IFormFile contract_upload, IFormFile nda_upload, IFormFile proposal_upload)
        {
            try
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);
                Data.EditID = userinfo.UserID.ToString();
                Data.InvitationToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                var allowedDocs = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };
                var fileSizeLimit = 10 * 1024 * 1024;

                if (contract_upload != null && !IsFileValid(contract_upload, allowedDocs, fileSizeLimit))
                    return Json(new { status = "error", message = "Invalid Contract Document" });

                if (nda_upload != null && !IsFileValid(nda_upload, new[] { ".pdf", ".docx" }, fileSizeLimit))
                    return Json(new { status = "error", message = "Invalid NDA Document" });

                if (proposal_upload != null && !IsFileValid(proposal_upload, allowedDocs, fileSizeLimit))
                    return Json(new { status = "error", message = "Invalid Proposal Document" });

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadedfiles/ClientDocs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePrefix = $"{Data.InvitationToken}_{DateTime.Now:yyyyMMddHHmmss}";

                Data.contract_upload = SaveFile(contract_upload, uploadsFolder, "Contract_" + filePrefix);
                Data.nda_upload = SaveFile(nda_upload, uploadsFolder, "NDA_" + filePrefix);
                Data.proposal_upload = SaveFile(proposal_upload, uploadsFolder, "Proposal_" + filePrefix);

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveClientonBoarding", "Trans");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "File upload failed: " + ex.Message });
            }

        }
        #endregion

        #region SaveContractForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveContractForm([FromBody] ContractForm Data)
        {
            try
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);
                Data.EditID = userinfo.UserID.ToString();

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveContractForm", "Trans");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Something went wrong! " + ex.Message });
            }

        }
        #endregion

        #region SaveBillingForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveBillingForm([FromBody] BillingForm Data)
        {
            try
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);
                Data.EditID = userinfo.UserID.ToString();

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveBillingForm", "Trans");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Something went wrong! " + ex.Message });
            }

        }
        #endregion

        #region SaveRecurringBillingForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveRecurringBillingForm([FromBody] RecurringBillingForm Data)
        {
            try
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);
                Data.EditID = userinfo.UserID.ToString();

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveRecurringBillingForm", "Trans");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Something went wrong! " + ex.Message });
            }

        }
        #endregion

        #region SaveMilestone
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        [DisableRequestSizeLimit]
        public IActionResult SaveMilestone([FromForm] MilestoneForm Data, IFormFile docUpload)
        {
            try
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);
                Data.EditID = userinfo.UserID.ToString();
                Data.InvitationToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                var allowedDocs = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };
                var fileSizeLimit = 10 * 1024 * 1024;

                if (docUpload != null && !IsFileValid(docUpload, allowedDocs, fileSizeLimit))
                    return Json(new { status = "error", message = "Invalid Document" });

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadedfiles/MilestoneDocs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePrefix = $"{Data.InvitationToken}_{DateTime.Now:yyyyMMddHHmmss}";

                Data.docUpload = SaveFile(docUpload, uploadsFolder, "Milestone_" + filePrefix);

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveMilestone", "Trans");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "File upload failed: " + ex.Message });
            }

        }
        #endregion

        #region SaveInvoiceForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        [DisableRequestSizeLimit]
        public IActionResult SaveInvoiceForm([FromForm] InvoiceForm Data, IFormFile attach1_upload, IFormFile attach2_upload)
        {
            try
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);
                Data.EditID = userinfo.UserID.ToString();
                Data.InvitationToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                var allowedDocs = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };
                var fileSizeLimit = 10 * 1024 * 1024;

                if (attach1_upload != null && !IsFileValid(attach1_upload, allowedDocs, fileSizeLimit))
                    return Json(new { status = "error", message = "Invalid Document" });

                if (attach2_upload != null && !IsFileValid(attach2_upload, new[] { ".pdf", ".docx" }, fileSizeLimit))
                    return Json(new { status = "error", message = "Invalid Document" });


                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadedfiles/InvoiceDocs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePrefix = $"{Data.InvitationToken}_{DateTime.Now:yyyyMMddHHmmss}";

                Data.attach1_upload = SaveFile(attach1_upload, uploadsFolder, "Inv_" + filePrefix);
                Data.attach2_upload = SaveFile(attach2_upload, uploadsFolder, "Support_" + filePrefix);

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveInvoiceForm", "Trans");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "File upload failed: " + ex.Message });
            }

        }

        #endregion

        #region SavePaymentForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        [DisableRequestSizeLimit]
        public IActionResult SavePaymentForm([FromForm] PaymentForm Data, IFormFile receipt_upload)
        {
            try
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);
                Data.EditID = userinfo.UserID.ToString();
                Data.InvitationToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                var allowedDocs = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };
                var fileSizeLimit = 10 * 1024 * 1024;

                if (receipt_upload != null && !IsFileValid(receipt_upload, allowedDocs, fileSizeLimit))
                    return Json(new { status = "error", message = "Invalid Receipt Document" });

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadedfiles/PaymentDocs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePrefix = $"{Data.InvitationToken}_{DateTime.Now:yyyyMMddHHmmss}";

                Data.receipt_upload = SaveFile(receipt_upload, uploadsFolder, "Receipt_" + filePrefix);

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SavePaymentForm", "Trans");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "File upload failed: " + ex.Message });
            }

        }
        #endregion        

        #region SaveARExceptionForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult SaveARExceptionForm([FromBody] ARExceptionForm Data)
        {
            try
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);
                Data.EditID = userinfo.UserID.ToString();

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveARExceptionForm", "Trans");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Something went wrong! " + ex.Message });
            }

        }
        #endregion

        #region SaveRenewalForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        [DisableRequestSizeLimit]
        public IActionResult SaveRenewalForm([FromForm] RenewalForm Data, IFormFile doc_upload)
        {
            try
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                //if (userinfo.UserTypeID.ToString() == Data.UserTypeID) return BadRequest("Cannot Create Account For Type " + userinfo.UserType);
                Data.EditID = userinfo.UserID.ToString();
                Data.InvitationToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                var allowedDocs = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };
                var fileSizeLimit = 10 * 1024 * 1024;

                if (doc_upload != null && !IsFileValid(doc_upload, allowedDocs, fileSizeLimit))
                    return Json(new { status = "error", message = "Invalid Renewal Document" });

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadedfiles/RenewalDocs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePrefix = $"{Data.InvitationToken}_{DateTime.Now:yyyyMMddHHmmss}";

                Data.doc_upload = SaveFile(doc_upload, uploadsFolder, "Renewal_" + filePrefix);

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "SaveRenewalForm", "Trans");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "File upload failed: " + ex.Message });
            }

        }

        #endregion



        // View All Grid .....
        #region ViewClientForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetViewAllClientForm()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetViewAllClientForm", "Trans");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region ViewContractForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetViewAllContractForm()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetViewAllContractForm", "Trans");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region ViewBillingForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetViewAllBillingForm()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetViewAllBillingForm", "Trans");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region ViewRecurringBillingForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetViewAllRecurringBillingForm()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetViewAllRecurringBillingForm", "Trans");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region ViewMilestoneForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetViewAllMilestoneForm()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetViewAllMilestoneForm", "Trans");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region ViewInvoiceForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetViewAllInvoiceForm()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetViewAllInvoiceForm", "Trans");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region ViewPaymentForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetViewAllPaymentForm()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetViewAllPaymentForm", "Trans");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region ViewARExceptionForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetViewAllARExceptionForm()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetViewAllARExceptionForm", "Trans");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region ViewRenewalForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetViewAllRenewalForm()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetViewAllRenewalForm", "Trans");
            return new HttpResponseMessageResult(response);
        }
        #endregion




        // Fill Form in Edit Mode .....
        #region FillClientonBoarding
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetClientById(int clientId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?clientId=" + clientId, "GetClientById", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region FillContractForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetContractById(int contractId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?contractId=" + contractId, "GetContractById", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region FillBillingForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetBillingById(int billingId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?billingId=" + billingId, "GetBillingById", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region FillRecurringBillingForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetRecurringBillingById(int rbillingId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?rbillingId=" + rbillingId, "GetRecurringBillingById", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region FillMilestone
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetMilestoneById(int milestoneId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?milestoneId=" + milestoneId, "GetMilestoneById", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region FillInvoiceForm-Master
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetInvoiceById(int invoiceId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?invoiceId=" + invoiceId, "GetInvoiceById", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region FillInvoiceForm-Detail
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetInvoiceDetailByInvoiceId(int invoiceId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?invoiceId=" + invoiceId, "GetInvoiceDetailByInvoiceId", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region FillPaymentForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetPaymentFormById(int paymentId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?paymentId=" + paymentId, "GetPaymentFormById", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region FillARExceptionForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetARExceptionById(int exceptionId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?exceptionId=" + exceptionId, "GetARExceptionById", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region FillRenewalForm
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetRenewalFormById(int renewId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?renewId=" + renewId, "GetRenewalFormById", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion


        // Delete Api .....
        #region DeleteClientForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteClientForm([FromBody] DeleteData Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteClientForm", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region DeleteContractForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteContractForm([FromBody] DeleteData Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteContractForm", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region DeleteBillingForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteBillingForm([FromBody] DeleteData Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteBillingForm", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region DeleteRecurringBillingForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteRecurringBillingForm([FromBody] DeleteData Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteRecurringBillingForm", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region DeleteMilestoneForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteMilestoneForm([FromBody] DeleteData Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteMilestoneForm", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region DeleteInvoiceForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteInvoiceForm([FromBody] DeleteData Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteInvoiceForm", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region DeletePaymentForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeletePaymentForm([FromBody] DeleteData Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeletePaymentForm", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region DeleteARExceptionForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteARExceptionForm([FromBody] DeleteData Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteARExceptionForm", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region DeleteRenewalForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult DeleteRenewalForm([FromBody] DeleteData Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "DeleteRenewalForm", "Trans");
            return new HttpResponseMessageResult(response);
        }

        #endregion


        // Common functions .....
        #region Common functions
        private bool IsFileValid(IFormFile file, string[] allowedExtensions, int maxSize)
        {
            if (file == null) return true;
            var ext = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(ext) && file.Length <= maxSize;
        }

        private string SaveFile(IFormFile file, string folder, string customName)
        {
            if (file == null) return null;

            string extension = Path.GetExtension(file.FileName);
            string uniqueName = customName + extension;
            string filePath = Path.Combine(folder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return uniqueName;
        }

        #endregion



        #endregion


        #region BillingRevenue OneForm_All Tab

        #region FillExistingClient
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult FillExistingClient(string searchValue)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet(
                "?searchValue=" + Uri.EscapeDataString(searchValue ?? ""),
                "FillExistingClient",
                "BillingRevenue"
            );

            return new HttpResponseMessageResult(response);
        }


        #endregion

        #region FillClientContractForEdit
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult FillClientContractForEdit(int clientId, int contractId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?clientId=" + clientId + "&contractId=" + contractId,
                                                "FillClientContractForEdit", "BillingRevenue");

            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region FillInvoiceForEditManual
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult FillInvoiceForEditManual(int clientId, int contractId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?clientId=" + clientId + "&contractId=" + contractId, "FillInvoiceForEditManual", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region FillPaymentForEdit
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult FillPaymentForEdit(int clientId, int contractId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?clientId=" + clientId + "&contractId=" + contractId, "FillPaymentForEdit", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region GetLatestContractByClient
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetLatestContractByClient(int clientId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?clientId=" + clientId, "GetLatestContractByClient", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region LoadMaxClientID
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult LoadMaxClientID()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "LoadMaxClientID", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region LoadMaxContractID
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult LoadMaxContractID()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "LoadMaxContractID", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region LoadMaxInvoiceID
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult LoadMaxInvoiceID()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet(null, "LoadMaxInvoiceID", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region SaveClientonBoarding
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> BR_SaveClientonBoarding([FromForm] BR_ClientInfo Data)
        {
            try
            {
                if (Data == null) return BadRequest("Form data is empty.");

                UserInfo userinfo = _requestClient.GetUserInformation();
                Data.EditID = userinfo?.UserID.ToString() ?? "1";
                Data.InvitationToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                // 1. File Validation Rules
                var allowedDocs = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx", ".doc" };
                long fileSizeLimit = 10 * 1024 * 1024; // 10MB
                
                // Existing filename agar edit hai to yahan aa jayega
                string ndaFileName = Data.nda_filename ?? "";
                string taxFileName = Data.tax_verify_filename ?? "";


                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadedfiles", "ClientDocs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePrefix = $"{Data.InvitationToken}_{DateTime.Now:yyyyMMddHHmmss}";

                // 1. Process NDA File
                if (Data.nda_upload != null && Data.nda_upload.Length > 0)
                {
                    var ext = Path.GetExtension(Data.nda_upload.FileName).ToLower();

                    if (!allowedDocs.Contains(ext) || Data.nda_upload.Length > fileSizeLimit)
                    {
                        // Filesize calculation in MBs for clearer display
                        double fileSizeInMB = Math.Round((double)Data.nda_upload.Length / (1024 * 1024), 2);
                        double maxLimitInMB = fileSizeLimit / (1024 * 1024);

                        string errorMessage = "";

                        if (!allowedDocs.Contains(ext))
                        {
                            errorMessage = $"Invalid NDA file format '{ext}'. Allowed formats: {string.Join(", ", allowedDocs)}.";
                        }
                        else if (Data.nda_upload.Length > fileSizeLimit)
                        {
                            errorMessage = $"NDA file size exceeded. Selected file size: {fileSizeInMB} MB. Maximum allowed limit: {maxLimitInMB} MB.";
                        }

                        return BadRequest(new
                        {
                            success = false,
                            title = "Validation Error",
                            field = "nda_upload",
                            message = errorMessage
                        });
                    }

                    ndaFileName = $"{filePrefix}_NDA{ext}";
                    string ndaPath = Path.Combine(uploadsFolder, ndaFileName);

                    using (var stream = new FileStream(ndaPath, FileMode.Create))
                    {
                        await Data.nda_upload.CopyToAsync(stream);
                    }
                    // Model ke file paths badalne ke liye hum hidden properties use kar sakte hain ya internal mapping.
                }

                // 2. Process Tax Verification File
                if (Data.tax_verify != null && Data.tax_verify.Length > 0)
                {
                    var ext = Path.GetExtension(Data.tax_verify.FileName).ToLower();

                    if (!allowedDocs.Contains(ext) || Data.tax_verify.Length > fileSizeLimit)
                    {
                        // Filesize calculation in MBs for clearer display
                        double fileSizeInMB = Math.Round((double)Data.tax_verify.Length / (1024 * 1024), 2);
                        double maxLimitInMB = fileSizeLimit / (1024 * 1024);

                        string errorMessage = "";

                        if (!allowedDocs.Contains(ext))
                        {
                            errorMessage = $"Invalid Tax Verify Document file format '{ext}'. Allowed formats: {string.Join(", ", allowedDocs)}.";
                        }
                        else if (Data.tax_verify.Length > fileSizeLimit)
                        {
                            errorMessage = $"Tax Verify Document file size exceeded. Selected file size: {fileSizeInMB} MB. Maximum allowed limit: {maxLimitInMB} MB.";
                        }

                        return BadRequest(new
                        {
                            success = false,
                            title = "Validation Error",
                            field = "tax_verify",
                            message = errorMessage
                        });
                    }

                    taxFileName = $"{filePrefix}_Tax{ext}";
                    string taxPath = Path.Combine(uploadsFolder, taxFileName);

                    using (var stream = new FileStream(taxPath, FileMode.Create))
                    {
                        await Data.tax_verify.CopyToAsync(stream);
                    }
                }

                // NOTE: Aage agar external HttpClient use karna hai, toh IFormFile internal process 
                // hone ke baad serialize nahi hota. Agar server pe save kar lia hai toh properties 
                // ko null kar dein taake serialization crash na ho.
                Data.nda_filename = ndaFileName;
                Data.tax_verify_filename = taxFileName;

                Data.nda_upload = null;
                Data.tax_verify = null;

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "BR_SaveClientonBoarding", "BillingRevenue");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "File upload failed: " + ex.Message });
            }
        }
        #endregion

        #region SaveContractForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public async Task<IActionResult> BR_SaveContractForm([FromForm] BR_ContractInfo Data)
        {
            try
            {
                if (Data == null) return BadRequest("Form data is empty.");

                UserInfo userinfo = _requestClient.GetUserInformation();
                Data.EditID = userinfo?.UserID.ToString() ?? "1";
                Data.InvitationToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                // 1. File Validation Rules
                var allowedDocs = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx", ".doc" };
                long fileSizeLimit = 10 * 1024 * 1024; // 10MB

                // Existing filename agar edit hai to yahan aa jayega
                string doc1FileName = Data.doc1_filename ?? "";
                string doc2FileName = Data.doc2_filename ?? "";
                string doc3FileName = Data.doc3_filename ?? "";
                string doc4FileName = Data.doc4_filename ?? "";


                string docPath = "";
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadedfiles", "ContractDocs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePrefix = $"{Data.InvitationToken}_{DateTime.Now:yyyyMMddHHmmss}";

                // 1. Process Doc1 File
                if (Data.doc1_upload != null && Data.doc1_upload.Length > 0)
                {
                    var ext = Path.GetExtension(Data.doc1_upload.FileName).ToLower();

                    if (!allowedDocs.Contains(ext) || Data.doc1_upload.Length > fileSizeLimit)
                    {
                        double fileSizeInMB = Math.Round((double)Data.doc1_upload.Length / (1024 * 1024), 2);
                        double maxLimitInMB = fileSizeLimit / (1024 * 1024);
                        string errorMessage = !allowedDocs.Contains(ext)
                            ? $"Invalid Doc1 file format '{ext}'. Allowed formats: {string.Join(", ", allowedDocs)}."
                            : $"Doc1 file size exceeded. Selected file size: {fileSizeInMB} MB. Maximum allowed limit: {maxLimitInMB} MB.";

                        return BadRequest(new
                        {
                            success = false,
                            title = "Validation Error",
                            field = "doc1_upload",
                            message = errorMessage
                        });
                    }

                    doc1FileName = $"{filePrefix}_Doc1{ext}";
                    docPath = Path.Combine(uploadsFolder, doc1FileName);

                    using (var stream = new FileStream(docPath, FileMode.Create))
                    {
                        await Data.doc1_upload.CopyToAsync(stream);
                    }
                    // Model ke file paths badalne ke liye hum hidden properties use kar sakte hain ya internal mapping.
                }

                // 2. Process Doc2 File
                if (Data.doc2_upload != null && Data.doc2_upload.Length > 0)
                {
                    var ext = Path.GetExtension(Data.doc2_upload.FileName).ToLower();
                    
                    if (!allowedDocs.Contains(ext) || Data.doc2_upload.Length > fileSizeLimit)
                    {
                        double fileSizeInMB = Math.Round((double)Data.doc2_upload.Length / (1024 * 1024), 2);
                        double maxLimitInMB = fileSizeLimit / (1024 * 1024);
                        string errorMessage = !allowedDocs.Contains(ext)
                            ? $"Invalid Doc2 file format '{ext}'. Allowed formats: {string.Join(", ", allowedDocs)}."
                            : $"Doc2 file size exceeded. Selected file size: {fileSizeInMB} MB. Maximum allowed limit: {maxLimitInMB} MB.";

                        return BadRequest(new
                        {
                            success = false,
                            title = "Validation Error",
                            field = "doc2_upload",
                            message = errorMessage
                        });
                    }

                    doc2FileName = $"{filePrefix}_Doc2{ext}";
                    docPath = Path.Combine(uploadsFolder, doc2FileName);

                    using (var stream = new FileStream(docPath, FileMode.Create))
                    {
                        await Data.doc2_upload.CopyToAsync(stream);
                    }
                    // Model ke file paths badalne ke liye hum hidden properties use kar sakte hain ya internal mapping.
                }

                // 3. Process Doc3 File
                if (Data.doc3_upload != null && Data.doc3_upload.Length > 0)
                {
                    var ext = Path.GetExtension(Data.doc3_upload.FileName).ToLower();
                    
                    if (!allowedDocs.Contains(ext) || Data.doc3_upload.Length > fileSizeLimit)
                    {
                        double fileSizeInMB = Math.Round((double)Data.doc3_upload.Length / (1024 * 1024), 2);
                        double maxLimitInMB = fileSizeLimit / (1024 * 1024);
                        string errorMessage = !allowedDocs.Contains(ext)
                            ? $"Invalid Doc3 file format '{ext}'. Allowed formats: {string.Join(", ", allowedDocs)}."
                            : $"Doc3 file size exceeded. Selected file size: {fileSizeInMB} MB. Maximum allowed limit: {maxLimitInMB} MB.";

                        return BadRequest(new
                        {
                            success = false,
                            title = "Validation Error",
                            field = "doc3_upload",
                            message = errorMessage
                        });
                    }

                    doc3FileName = $"{filePrefix}_Doc3{ext}";
                    docPath = Path.Combine(uploadsFolder, doc3FileName);

                    using (var stream = new FileStream(docPath, FileMode.Create))
                    {
                        await Data.doc3_upload.CopyToAsync(stream);
                    }
                    // Model ke file paths badalne ke liye hum hidden properties use kar sakte hain ya internal mapping.
                }

                // 4. Process Doc4 File
                if (Data.doc4_upload != null && Data.doc4_upload.Length > 0)
                {
                    var ext = Path.GetExtension(Data.doc4_upload.FileName).ToLower();
                    
                    if (!allowedDocs.Contains(ext) || Data.doc4_upload.Length > fileSizeLimit)
                    {
                        double fileSizeInMB = Math.Round((double)Data.doc4_upload.Length / (1024 * 1024), 2);
                        double maxLimitInMB = fileSizeLimit / (1024 * 1024);
                        string errorMessage = !allowedDocs.Contains(ext)
                            ? $"Invalid Doc4 file format '{ext}'. Allowed formats: {string.Join(", ", allowedDocs)}."
                            : $"Doc4 file size exceeded. Selected file size: {fileSizeInMB} MB. Maximum allowed limit: {maxLimitInMB} MB.";

                        return BadRequest(new
                        {
                            success = false,
                            title = "Validation Error",
                            field = "doc4_upload",
                            message = errorMessage
                        });
                    }

                    doc4FileName = $"{filePrefix}_Doc4{ext}";
                    docPath = Path.Combine(uploadsFolder, doc4FileName);

                    using (var stream = new FileStream(docPath, FileMode.Create))
                    {
                        await Data.doc4_upload.CopyToAsync(stream);
                    }
                    // Model ke file paths badalne ke liye hum hidden properties use kar sakte hain ya internal mapping.
                }


                // NOTE: Aage agar external HttpClient use karna hai, toh IFormFile internal process 
                // hone ke baad serialize nahi hota. Agar server pe save kar lia hai toh properties 
                // ko null kar dein taake serialization crash na ho.
                Data.doc1_filename = doc1FileName;
                Data.doc2_filename = doc2FileName;
                Data.doc3_filename = doc3FileName;
                Data.doc4_filename = doc4FileName;

                Data.doc1_upload = null;
                Data.doc2_upload = null;
                Data.doc3_upload = null;
                Data.doc4_upload = null;


                var milestonesJson = Request.Form["Milestones"];
                if (!string.IsNullOrWhiteSpace(milestonesJson))
                {
                    Data.Milestones = JsonConvert.DeserializeObject<List<BR_ContractMilestone>>(milestonesJson);
                }

                var count = Data.Milestones?.Count ?? 0;

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "BR_SaveContractForm", "BillingRevenue");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "File upload failed: " + ex.Message });
            }

        }

        #endregion

        #region SaveInvoideForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public async Task<IActionResult> BR_SaveInvoideForm([FromForm] BR_InvoiceInfo Data)
        {
            try
            {
                if (Data == null) 
                    return BadRequest("Form data is empty.");

                UserInfo userinfo = _requestClient.GetUserInformation();
                Data.EditID = userinfo?.UserID.ToString() ?? "1";
                Data.InvitationToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                // 1. File Validation Rules
                var allowedDocs = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx", ".doc" };
                long fileSizeLimit = 10 * 1024 * 1024; // 10MB

                // Existing filename agar edit hai to yahan aa jayega
                string doc1FileName = Data.attach1_filename ?? "";
                string doc2FileName = Data.attach2_filename ?? "";

                string docPath = "";
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadedfiles", "InvoiceDocs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePrefix = $"{Data.InvitationToken}_{DateTime.Now:yyyyMMddHHmmss}";

                // 1. Process Doc1 File
                if (Data.attach1_upload != null && Data.attach1_upload.Length > 0)
                {
                    var ext = Path.GetExtension(Data.attach1_upload.FileName).ToLower();

                    if (!allowedDocs.Contains(ext) || Data.attach1_upload.Length > fileSizeLimit)
                    {
                        double fileSizeInMB = Math.Round((double)Data.attach1_upload.Length / (1024 * 1024), 2);
                        double maxLimitInMB = fileSizeLimit / (1024 * 1024);
                        string errorMessage = !allowedDocs.Contains(ext)
                            ? $"Invalid Doc1 file format '{ext}'. Allowed formats: {string.Join(", ", allowedDocs)}."
                            : $"Doc1 file size exceeded. Selected file size: {fileSizeInMB} MB. Maximum allowed limit: {maxLimitInMB} MB.";

                        return BadRequest(new
                        {
                            success = false,
                            title = "Validation Error",
                            field = "attach1_upload",
                            message = errorMessage
                        });
                    }

                    doc1FileName = $"{filePrefix}_Doc1{ext}";
                    docPath = Path.Combine(uploadsFolder, doc1FileName);

                    using (var stream = new FileStream(docPath, FileMode.Create))
                    {
                        await Data.attach1_upload.CopyToAsync(stream);
                    }
                    // Model ke file paths badalne ke liye hum hidden properties use kar sakte hain ya internal mapping.
                }

                // 1. Process Doc2 File
                if (Data.attach2_upload != null && Data.attach2_upload.Length > 0)
                {
                    var ext = Path.GetExtension(Data.attach2_upload.FileName).ToLower();

                    if (!allowedDocs.Contains(ext) || Data.attach2_upload.Length > fileSizeLimit)
                    {
                        double fileSizeInMB = Math.Round((double)Data.attach2_upload.Length / (1024 * 1024), 2);
                        double maxLimitInMB = fileSizeLimit / (1024 * 1024);
                        string errorMessage = !allowedDocs.Contains(ext)
                            ? $"Invalid Doc1 file format '{ext}'. Allowed formats: {string.Join(", ", allowedDocs)}."
                            : $"Doc1 file size exceeded. Selected file size: {fileSizeInMB} MB. Maximum allowed limit: {maxLimitInMB} MB.";

                        return BadRequest(new
                        {
                            success = false,
                            title = "Validation Error",
                            field = "attach2_upload",
                            message = errorMessage
                        });
                    }

                    doc2FileName = $"{filePrefix}_Doc2{ext}";
                    docPath = Path.Combine(uploadsFolder, doc2FileName);

                    using (var stream = new FileStream(docPath, FileMode.Create))
                    {
                        await Data.attach2_upload.CopyToAsync(stream);
                    }
                    // Model ke file paths badalne ke liye hum hidden properties use kar sakte hain ya internal mapping.
                }

                // NOTE: Aage agar external HttpClient use karna hai, toh IFormFile internal process 
                // hone ke baad serialize nahi hota. Agar server pe save kar lia hai toh properties 
                // ko null kar dein taake serialization crash na ho.
                Data.attach1_filename = doc1FileName;
                Data.attach2_filename = doc2FileName;

                Data.attach1_upload = null;
                Data.attach2_upload = null;

                ////var invoice_details = Request.Form["invoice_details"];
                ////if (!string.IsNullOrWhiteSpace(invoice_details))
                ////{
                ////    Data.invoice_details = JsonConvert.DeserializeObject<List<BR_InvoiceDetail>>(invoice_details);
                ////}
                ////var count = Data.invoice_details?.Count ?? 0;

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "BR_SaveInvoideForm", "BillingRevenue");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Something went wrong! " + ex.Message });
            }

        }

        #endregion

        #region SavePaymentForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public async Task<IActionResult> BR_SavePaymentForm([FromForm] BR_PaymentInfo Data)
        {
            try
            {
                if (Data == null) return BadRequest("Form data is empty.");

                UserInfo userinfo = _requestClient.GetUserInformation();
                Data.EditID = userinfo?.UserID.ToString() ?? "1";
                Data.InvitationToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                // 1. File Validation Rules
                var allowedDocs = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx", ".doc" };
                long fileSizeLimit = 10 * 1024 * 1024; // 10MB

                // Existing filename agar edit hai to yahan aa jayega
                string receipt_filename = Data.receipt_upload ?? "";


                string docPath = "";
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadedfiles", "PaymentDocs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePrefix = $"{Data.InvitationToken}_{DateTime.Now:yyyyMMddHHmmss}";

                // 1. Process Doc1 File
                if (Data.receipt_file != null && Data.receipt_file.Length > 0)
                {
                    var ext = Path.GetExtension(Data.receipt_file.FileName).ToLower();
                    
                    if (!allowedDocs.Contains(ext) || Data.receipt_file.Length > fileSizeLimit)
                    {
                        double fileSizeInMB = Math.Round((double)Data.receipt_file.Length / (1024 * 1024), 2);
                        double maxLimitInMB = fileSizeLimit / (1024 * 1024);

                        string errorMessage = !allowedDocs.Contains(ext)
                            ? $"Invalid Receipt file format '{ext}'. Allowed formats: {string.Join(", ", allowedDocs)}."
                            : $"Receipt file size exceeded. Selected file size: {fileSizeInMB} MB. Maximum allowed limit: {maxLimitInMB} MB.";

                        return BadRequest(new
                        {
                            success = false,
                            title = "Validation Error",
                            field = "receipt_file",
                            message = errorMessage
                        });
                    }

                    receipt_filename = $"{filePrefix}_Doc1{ext}";
                    docPath = Path.Combine(uploadsFolder, receipt_filename);

                    using (var stream = new FileStream(docPath, FileMode.Create))
                    {
                        await Data.receipt_file.CopyToAsync(stream);
                    }
                    // Model ke file paths badalne ke liye hum hidden properties use kar sakte hain ya internal mapping.
                }


                // NOTE: Aage agar external HttpClient use karna hai, toh IFormFile internal process 
                // hone ke baad serialize nahi hota. Agar server pe save kar lia hai toh properties 
                // ko null kar dein taake serialization crash na ho.
                Data.receipt_upload = receipt_filename;

                Data.receipt_file = null;

                var paymentdtlJson = Request.Form["PaymentDtl"];
                if (!string.IsNullOrWhiteSpace(paymentdtlJson))
                {
                    Data.PaymentDetails = JsonConvert.DeserializeObject<List<BR_PaymentDetail>>(paymentdtlJson);
                }

                var count = Data.PaymentDetails?.Count ?? 0;

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "BR_SavePaymentForm", "BillingRevenue");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "File upload failed: " + ex.Message });
            }

        }

        #endregion

        #region SaveARException
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public async Task<IActionResult> BR_SaveARException([FromForm] BR_ARExceptionInfo Data)
        {
            try
            {
                if (Data == null) return BadRequest("Form data is empty.");

                UserInfo userinfo = _requestClient.GetUserInformation();
                Data.EditID = userinfo?.UserID.ToString() ?? "1";

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "BR_SaveARException", "BillingRevenue");
                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Save failed: " + ex.Message });
            }
        }
        #endregion

        #region SaveRenewalForm
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public async Task<IActionResult> BR_SaveRenewalForm([FromForm] BR_RenewalInfo Data)
        {
            try
            {
                if (Data == null) return BadRequest("Form data is empty.");

                UserInfo userinfo = _requestClient.GetUserInformation();
                Data.EditID = userinfo?.UserID.ToString() ?? "1";
                Data.InvitationToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                var allowedDocs = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx", ".doc" };
                long fileSizeLimit = 10 * 1024 * 1024;

                string doc_filename = Data.doc_upload ?? "";
                string docPath = "";
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadedfiles", "RenewalDocs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePrefix = $"{Data.InvitationToken}_{DateTime.Now:yyyyMMddHHmmss}";

                var renewalDocFile = Request.Form.Files.GetFile("renewal_doc_file");

                if (renewalDocFile != null && renewalDocFile.Length > 0)
                {
                    var ext = Path.GetExtension(renewalDocFile.FileName).ToLower();

                    if (!allowedDocs.Contains(ext) || renewalDocFile.Length > fileSizeLimit)
                    {
                        double fileSizeInMB = Math.Round((double)renewalDocFile.Length / (1024 * 1024), 2);
                        double maxLimitInMB = fileSizeLimit / (1024 * 1024);
                        string errorMessage = !allowedDocs.Contains(ext)
                            ? $"Invalid Renewal Document format '{ext}'. Allowed formats: {string.Join(", ", allowedDocs)}."
                            : $"Renewal Document size exceeded. Selected file size: {fileSizeInMB} MB. Maximum allowed limit: {maxLimitInMB} MB.";

                        return BadRequest(new
                        {
                            success = false,
                            title = "Validation Error",
                            field = "renewal_doc_file",
                            message = errorMessage
                        });
                    }

                    doc_filename = $"{filePrefix}_RenewalDoc{ext}";
                    docPath = Path.Combine(uploadsFolder, doc_filename);

                    using (var stream = new FileStream(docPath, FileMode.Create))
                    {
                        await renewalDocFile.CopyToAsync(stream);
                    }
                }

                Data.doc_upload = doc_filename;

                string jsonString = JsonConvert.SerializeObject(Data);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "BR_SaveRenewalForm", "BillingRevenue");
                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "File upload failed: " + ex.Message });
            }
        }
        #endregion


        //Invoice Grid
        #region GetAllInvoicesGrid
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetAllInvoicesGrid(int client_id, int contract_id)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?client_id=" + client_id + "&contract_id=" + contract_id + "&UserID = " + _info.UserID, "GetAllInvoicesGrid", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }

        #endregion


        //Payment Grid
        #region GetAllUnPaidInvoiceList
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetAllUnPaidInvoiceList(int client_id, int contract_id)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?client_id=" + client_id + "&contract_id=" + contract_id + "&UserID = " + _info.UserID, "GetAllUnPaidInvoiceList", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region GetOverdueInvoices
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetOverdueInvoices(int clientId, int contractId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?clientId=" + clientId + "&contractId=" + contractId,
                                                "GetOverdueInvoices", "BillingRevenue");

            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region GetARExceptionGrid
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetARExceptionGrid(int clientId, int contractId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?clientId=" + clientId + "&contractId=" + contractId, "GetARExceptionGrid", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region GetRenewalGrid
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetRenewalGrid(int clientId, int contractId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?clientId=" + clientId + "&contractId=" + contractId, "GetRenewalGrid", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }

        #endregion

        #region GetClientDashboard
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetClientDashboard(int clientId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?clientId=" + clientId, "GetClientDashboard", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }

        #endregion


        //ConvertRenewalToContract
        #region ConvertRenewalToContract
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult ConvertRenewalToContract(int renewId)
        {
            try
            {
                UserInfo userinfo = _requestClient.GetUserInformation();
                int userId = userinfo?.UserID ?? 1;

                var payload = new
                {
                    renew_id = renewId,
                    createdby = userId
                };

                string jsonString = JsonConvert.SerializeObject(payload);
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "ConvertRenewalToContract", "BillingRevenue");
                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Conversion failed: " + ex.Message });
            }
        }
        #endregion


        //Auto Invoice API endpoint
        #region Auto Generate Invoice
        //[HttpPost]
        //[TypeFilter(typeof(AllowedApiAccess))]
        //public IActionResult AutoGenerateInvoice()
        //{
        //    HttpResponseMessage response = _requestClient.UseHttpClientPost("", "AutoGenerateInvoice", "BillingRevenue");

        //    return new HttpResponseMessageResult(response);
        //}

        #endregion

        #region FillInvoiceForPrint
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult FillInvoiceForPrint(int invoiceId)
        {
            HttpResponseMessage response = _requestClient.UseHttpClientGet("?invoiceId=" + invoiceId, "FillInvoiceForPrint", "BillingRevenue");

            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region EmailInvoice
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EmailInvoice([FromForm] int invoiceId, [FromForm] IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("PDF file is required.");
            }


            try
            {
                string base64File = "";

                // File ko byte array aur phir Base64 string mein convert karein
                using (var ms = new MemoryStream())
                {
                    pdfFile.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    base64File = Convert.ToBase64String(fileBytes);
                }

                // Ek anonymous object banayein jo API ko bhejna hai
                var payload = new
                {
                    InvoiceId = invoiceId,
                    FileName = pdfFile.FileName,
                    FileBase64 = base64File
                };

                // Object ko JSON string mein badlein
                string jsonData = JsonConvert.SerializeObject(payload);

                // Aapka existing method bina kisi change ke chalega
                HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonData, "EmailInvoice", "BillingRevenue");

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in BaseController/EmailInvoice: {0}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region EmailTemplate
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult EmailTemplate(
            [FromForm] string test_email,
            [FromForm] int template_id,
            [FromForm] string subject,
            [FromForm] string body)
        {
            try
            {
                var payload = new
                {
                    TestEmail = test_email,
                    TemplateId = template_id,
                    Subject = subject,
                    Body = body
                };

                string jsonData = JsonConvert.SerializeObject(payload);

                HttpResponseMessage response =
                    _requestClient.UseHttpClientPost(
                        jsonData,
                        "EmailTemplate",
                        "BillingRevenue"
                    );

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error in BaseController/EmailTemplate: {0}",
                    ex.Message
                );

                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region GetAllClientList
        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetAllClientList()
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?UserID=" + _info.UserID, "GetAllClientList", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }
        #endregion

        #region ReportMenu

        [HttpGet]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult LoadClientReport(int clientId)
        {
            UserInfo _info = _requestClient.GetUserInformation();

            HttpResponseMessage response = _requestClient.UseHttpClientGet("?clientId=" + clientId, "LoadClientReport", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetClientReport([FromBody] ReportParam Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "GetClientReport", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetClientContractReport([FromBody] ReportParam Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "GetClientContractReport", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetClientInvoiceReport([FromBody] ReportParam Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "GetClientInvoiceReport", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }

        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetInvoiceDueReport([FromBody] ReportParam Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "GetInvoiceDueReport", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }


        //GetNextUnpaidInvoiceDue
        [HttpPost]
        [TypeFilter(typeof(AllowedApiAccess))]
        public IActionResult GetNextUnpaidInvoiceDue([FromBody] ReportParam Data)
        {
            UserInfo userinfo = _requestClient.GetUserInformation();
            Data.EditID = userinfo.UserID.ToString();
            string jsonString = JsonConvert.SerializeObject(Data);
            HttpResponseMessage response = _requestClient.UseHttpClientPost(jsonString, "GetNextUnpaidInvoiceDue", "BillingRevenue");
            return new HttpResponseMessageResult(response);
        }



        #endregion


        #endregion

    }

}
