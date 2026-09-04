using RevenueBillingApi.Models.Authentication;
using System.Security.Claims;

namespace RevenueBillingApi.Repository
{
    public interface IJWTManagerRepository
    {
        Tokens Authenticate(UserDTO users);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
