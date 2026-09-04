using System.ComponentModel.DataAnnotations;

namespace RevenueBillingApp.Models.Authentication
{
    public class ChangePassword
    {
        [Required]
        public string oldpassword { get; set; }

        [Required]
        public string newpassword { get; set; }

        [Required]
        public string confirmpassword { get; set; }
    }
}