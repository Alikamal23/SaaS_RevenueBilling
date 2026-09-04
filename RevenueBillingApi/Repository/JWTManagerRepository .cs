using RevenueBillingApi.Models.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RevenueBillingApi.Repository
{
    public class JWTManagerRepository : IJWTManagerRepository
    {

        private readonly IConfiguration iconfiguration;
        private readonly UserDAL _userDAL;
        public JWTManagerRepository(IConfiguration iconfiguration, UserDAL userDAL)
        {
            this.iconfiguration = iconfiguration;
            _userDAL = userDAL;
        }
        public Tokens Authenticate(UserDTO users)
        {
            UserDTO user = _userDAL.ValidateUser(users.LoginName, users.UserPassword);

            if (user == null)
            {
                return null;
            }

            // Else we generate JSON Web Token
            var tokenexpirey = DateTime.UtcNow.AddMinutes(5);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("AM_JWT_KEY"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.UserEmail),
                    new Claim("UserID", user.UserID.ToString())
                }),
                Expires = tokenexpirey,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokeinitiatedatetime = DateTime.Now;
            var tokenexpirydatetime = DateTime.Now.AddMinutes(5);
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new Tokens
            {
                access_token = tokenHandler.WriteToken(token),
                token_type = "bearer",
                expires_in = tokenexpirey.ToString(),
                start_date_time = tokeinitiatedatetime,
                end_date_time = tokenexpirydatetime
            };

        }
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var Key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("AM_JWT_KEY"));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Key),
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }


            return principal;
        }
    }
}
