using Microsoft.AspNetCore.Mvc;

namespace RevenueBillingApp.Utility
{
    public class HttpResponseMessageResult : IActionResult
    {
        private readonly HttpResponseMessage _response;

        public HttpResponseMessageResult(HttpResponseMessage response)
        {
            _response = response;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = (int)_response.StatusCode;

            foreach (var header in _response.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            if (_response.Content != null)
            {
                foreach (var header in _response.Content.Headers)
                {
                    response.Headers[header.Key] = header.Value.ToArray();
                }

                await _response.Content.CopyToAsync(response.Body);
            }
        }
    }
}
