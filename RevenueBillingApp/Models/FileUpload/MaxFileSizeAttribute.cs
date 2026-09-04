using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using RevenueBillingApp.Models.Session;

namespace RevenueBillingApp.Models
{
    public class MaxFileSizeAttribute : ActionFilterAttribute
    {
        private int? _maxFileSize;
        private readonly int _bytessize = (1024 * 1024);
        private IConfiguration _configuration;
        private readonly Sessions _sessions;
        public MaxFileSizeAttribute(IConfiguration configuration, Sessions sessions)
        {
            _configuration = configuration;
            _sessions = sessions;
            _maxFileSize = 0;
        }
        //public int GetAllowedFileSize(string token)
        //{
        //    string content = string.Empty;
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //            client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AM_SERVICE_URL") + "Forms/GetAllowedFileSize");
        //            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //            var response = client.GetAsync(client.BaseAddress).Result.Content.ReadAsStringAsync().Result;
        //            DataTable dt = JsonConvert.DeserializeObject<DataTable>(response);
        //            content = dt.Rows[0]["file_size"].ToString();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("SystemActivityLog |" + ex.ToString());
        //    }
        //    return Convert.ToInt32(content);
        //}
        public int GetAllowedFileSize()
        {
            int filesize = 5;
            return filesize;
        }
        public override void OnActionExecuting(ActionExecutingContext validationContext)
        {
            bool Result = true;
            var request = validationContext.HttpContext.Request;

            //ClaimsPrincipal claimsPrincipal = validationContext.HttpContext.User;
            //var UserID = (from c in claimsPrincipal.Claims where c.Type == "UserID" select c.Value).FirstOrDefault();
            //var UniqueKey = (from c in claimsPrincipal.Claims where c.Type == "Guid" select c.Value).FirstOrDefault();
            //SessionItems sessionItems = _sessions.GetSession(UniqueKey, UserID);
            //_maxFileSize = GetAllowedFileSize(sessionItems.authToken.access_token);

            _maxFileSize = GetAllowedFileSize();

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
                                var length = f.Length;
                                if (length > 0 && length > _maxFileSize * _bytessize)
                                {
                                    Result = false;
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    validationContext.Result = new BadRequestObjectResult(ex.Message);
                }
            }
            if (!Result)
            {
                validationContext.Result = new BadRequestObjectResult(GetErrorMessage());
            }
        }
        public string GetErrorMessage()
        {
            return $"Maximum allowed file size is {_maxFileSize} MB.";
        }
    }
}
