using RevenueBillingApi.Extensions;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace RevenueBillingApi.Repository
{
    public class OAuthSignalRConnection
    {
        private readonly DataEncryptor _dataencryptor;
        public OAuthSignalRConnection(DataEncryptor dataencryptor)
        {
            _dataencryptor = dataencryptor;
        }
        public Task RequestToken(MessageReceivedContext context)
        {
            var accessToken = context.Request.Query["access_token"];

            string decodedString = _dataencryptor.DecryptPassword(accessToken);
          
            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            //if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hubs/InternalBankHub")))
            if (!string.IsNullOrEmpty(decodedString))
            {
                // Read the token out of the query string
                context.Token = decodedString;
            }
            return Task.CompletedTask;
        }
    }
}
