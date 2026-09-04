namespace RevenueBillingApi.Models.Authentication
{
    public class UserDTO
    {
        public int? UserID { get; set; }
        public string? UserName { get; set; }
        public string? LoginName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPassword { get; set; }
        public int? RoleID { get; set; }
        public string? RoleName { get; set; }
        public int? DefaultFormID { get; set; }
        public string? DefaultFormName { get; set; }
        public string? SessionID { get; set; }
        public bool? IsLogin { get; set; }
        public bool? IsActive { get; set; }
    }
}
