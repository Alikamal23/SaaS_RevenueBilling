using RevenueBillingApi.Models.Authentication;
using RevenueBillingApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RevenueBillingApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IJWTManagerRepository _jWTManager;
        private readonly ILogger<AuthenticationController> _logger;
        public AuthenticationController(IJWTManagerRepository jWTManager, ILogger<AuthenticationController> logger)
        {
            this._jWTManager = jWTManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult token(UserDTO usersdata)
        {
            var token = _jWTManager.Authenticate(usersdata);

            if (token == null)
            {
                _logger.LogInformation("{0} {1} {2}", usersdata.LoginName, "AuthController/token", "Unauthorized");
                return Unauthorized();
            }
            _logger.LogInformation("{0} {1} {2}", usersdata.LoginName, "AuthController/token", "authorized");
            return Ok(token);
        }
    }
}
