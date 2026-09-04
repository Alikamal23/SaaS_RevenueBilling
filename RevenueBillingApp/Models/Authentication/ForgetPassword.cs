using System.ComponentModel.DataAnnotations;

namespace RevenueBillingApp.Models.Authentication
{
    public class ForgetPassword
    {
        [Required]
        public string UserEmail { get; set; }
    }
}