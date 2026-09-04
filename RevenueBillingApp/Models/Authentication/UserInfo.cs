namespace RevenueBillingApp.Models.Authentication
{
    public class UserInfo
    {
        public int? UserID { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPassword { get; set; }
        public int? UserTypeID { get; set; }
        public string? UserType { get; set; }
        public string? MobileNumber { get; set; }
        public int? RoleID { get; set; }
        public string? RoleName { get; set; }
        public int? DefaultFormID { get; set; }
        public string? DefaultFormName { get; set; }
        public string? FormDisplayName { get; set; }
        public string? SessionID { get; set; }
        public bool? IsLogin { get; set; }
        public bool? IsActive { get; set; }
        public string? UserImage { get; set; }
        public bool? IsReset { get; set; }
        public DateTime? PasswordExpiryDate { get; set; }
        public string? LoginName { get; set; }
        public bool IsAdUser { get; set; }
        public string? DomainName { get; set; }
        public DateTime? BlockTime { get; set; }
        public string? AttemptCount { get; set; }
        public bool? BlockType { get; set; }
        public int? RemainingExpiryDays { get; set; }
        public DateTime? SessionDateTime { get; set; }

    }
}