using RevenueBillingApp.Models.FileUpload;
using RevenueBillingApp.Models.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RevenueBillingApp.Controllers
{
    [Authorize(AuthenticationSchemes = "ASPXAUTH")]
    public class FileUploadController : Controller
    {
        private readonly Sessions _sessions;
        private readonly IWebHostEnvironment _environment;


        public FileUploadController(Sessions sessions, IWebHostEnvironment webHostEnvironment)
        {
            _sessions = sessions;
            _environment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
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


        [TypeFilter(typeof(AllowedExtensionsAttribute))]
        [HttpPost]
        public IActionResult UploadModelDocument(CustomerDocument doc)
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


        #region Classes
        public class CustomerDocument
        {
            public List<IFormFile>? files { get; set; }
        }
        #endregion


    }
}
