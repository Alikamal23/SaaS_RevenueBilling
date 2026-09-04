namespace RevenueBillingApp.Models.Authentication
{
    public class VerifyEmail
    {
        public bool? IsVerified { get; set; }
        public string? LoginName { get; set; }
        public DateTime? PasswordCreateDate { get; set; }
    }
}
