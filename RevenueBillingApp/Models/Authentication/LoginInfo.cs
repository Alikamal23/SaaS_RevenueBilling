using System.ComponentModel.DataAnnotations;

namespace RevenueBillingApp.Models.Authentication
{
    public class LoginInfo
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "LoginName")]
        public string LoginName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Remember Me")]
        public string RememberMe { get; set; }
    }
}