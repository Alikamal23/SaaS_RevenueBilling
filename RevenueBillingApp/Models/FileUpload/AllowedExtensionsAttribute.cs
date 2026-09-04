using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using RevenueBillingApp.Models.Session;
using System.Net;
using System.Net.Http.Headers;
using System.Data;
using Newtonsoft.Json;
using System.Security.Claims;


namespace RevenueBillingApp.Models.FileUpload
{
    public class AllowedExtensionsAttribute : ActionFilterAttribute
    {
        private string[]? _extensions;
        private IConfiguration _configuration;
        private readonly Sessions _sessions;
        public AllowedExtensionsAttribute(IConfiguration configuration, Sessions sessions)
        {
            _configuration = configuration;
            _sessions = sessions;
        }

        public string[] GetAllowedFileExtensions()
        {
            string content;
            string[] arrycontent_ = new string[] { };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            try
            {

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Sessions/FillLAllowedFilesExt");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = client.GetAsync(client.BaseAddress).Result.Content.ReadAsStringAsync().Result;
                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(response);
                    content = dt.Rows[0]["AllowedFileExtensions"].ToString();
                    arrycontent_ = content.Split(",");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SystemActivityLog |" + ex.ToString());
            }
            return arrycontent_;
        }

        public override void OnActionExecuting(ActionExecutingContext validationContext)
        {

            bool Result = true;
            var request = validationContext.HttpContext.Request;

            _extensions = GetAllowedFileExtensions();

            if (request.Method.ToLower() == "post")
            {
                try
                {
                    if (request.HasFormContentType)
                    {
                        var file = request.Form.Files;

                        if (file.Count > 0)
                        {
                            foreach (var f in file)
                            {
                                var extension = Path.GetExtension(f.FileName);
                                if (!_extensions.Contains(extension.ToLower()))
                                {
                                    Result = false;
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }

            if (!Result)
            {
                validationContext.Result = new BadRequestObjectResult(GetErrorMessage());
            }
        }

        public string GetErrorMessage()
        {
            return $"This extension is not allowed!";
        }

    }
}
