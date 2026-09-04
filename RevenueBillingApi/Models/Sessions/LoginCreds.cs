namespace RevenueBillingApi.Models.Sessions
{
    public class LoginCreds
    {
        public string? UserID { get; set; }
        public string? LoginName { get; set; }
        public string? RoleID { get; set; }
        public string? UserPassword { get; set; }
        public string? UserPassword_Old { get; set; }
        public string? UserPassword_New { get; set; }
    }
}
