using Microsoft.Extensions.Primitives;

namespace RevenueBillingApp.Middleware
{
    public sealed class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers.Remove("X-Powered-By");

            context.Response.Headers.Clear();
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            context.Response.Headers.Add("x-content-type-options", new StringValues("nosniff"));
            context.Response.Headers.Add("x-frame-options", new StringValues("DENY"));
            context.Response.Headers.Add("x-xss-protection", new StringValues("1; mode=block"));
            context.Response.Headers.Add("Permissions-Policy", "geolocation=(self)");
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";

            string stylesrcelem = " 'unsafe-inline' https://fonts.googleapis.com 'sha256-TotaJgQpi3uAcFIYGbWcMXZfL/6yP3V4SMJJLI/gF94=' 'sha256-47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=' 'sha256-UQBytKn0DQWyDg5/YC+FaQxonSsbQk4k0ErDHqBuhfw=' 'sha256-2iXqEuHSEQP1S2FDDTYaDeRVL3Q96bRqnCSjZlUqpNw=' 'sha256-YwStgWuzr/kqGmgy9cvuUIDSFeyx8ZqmV8CCexCdwo0=' 'sha256-FwG/QEDmtzyDO2c7Mskt+f1QFYU90sJRjn3Y6y9xcAg=' 'sha256-gGBiEYBtFtvyd9V1YznWoOUE6wlOLI3LA2I372SEWXc=' https://fonts.googleapis.com";

            string stylesrcattr = " 'unsafe-inline' 'unsafe-hashes' 'sha256-ZdHxw9eWtnxUb3mk6tBS+gIiVUPE3pGM470keHPDFlE=' 'sha256-BQ5eA/mw6jES31KSfh/A55TC7nzftLBWpZBzzDfwUrA=' 'sha256-FwG/QEDmtzyDO2c7Mskt+f1QFYU90sJRjn3Y6y9xcAg=' 'sha256-YwStgWuzr/kqGmgy9cvuUIDSFeyx8ZqmV8CCexCdwo0=' 'sha256-eJtrYeVWbmVy/9STt8BpA4s+1m/fPxrmS6f16u8JG6Y=' 'sha256-8TB0yXAPT48j7K7NpkQtjqJJZ0TkjoZXu2iXdfYXMyE=' 'sha256-DPppH8BAb+MFcX3MPEX4l/d4veqMGx/vKmDIf18LCcA=' 'sha256-n1bhJ4RuT2lJI6ZDFgJxh6l0t3bPq2X/7os91weIumg=' 'sha256-biLFinpqYMtWHmXfkA1BPeCY0/fNt46SAZ+BBk5YUog=' ";

            
            
            context.Response.Headers.Add("Content-Security-Policy", new StringValues(
                "base-uri 'self';" +
                "block-all-mixed-content;" +
                "child-src 'self';" +
                "connect-src 'self' " + _configuration["CONNECT_SRC"] + ";" +
                "default-src 'self';" +
                "font-src 'self' https://fonts.gstatic.com;" + 
                "form-action 'self';" +
                "frame-ancestors 'none';" +
                "frame-src 'self';" +
                "img-src 'self' data:;" +
                "manifest-src 'self';" +
                "media-src 'self';" +
                "object-src 'self';" +
                "script-src 'self';" +
                "script-src-attr 'self';" +
                "script-src-elem 'self';" +
                "style-src 'self' 'unsafe-inline';" +
                "style-src-attr 'self' 'unsafe-inline' https://fonts.googleapis.com;" +
"style-src-elem 'self' 'unsafe-inline' https://fonts.googleapis.com;"+
            "upgrade-insecure-requests;" +
                "worker-src 'self';"
                ));
            context.Response.Headers.Add("referrer-policy", new StringValues("strict-origin-when-cross-origin"));
            return _next(context);
        }
    }
}