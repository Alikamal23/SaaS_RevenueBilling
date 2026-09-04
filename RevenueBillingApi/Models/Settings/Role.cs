namespace RevenueBillingApi.Models.Settings
{
    public class Role
    {
        public string? UserID { get; set; }
        public string? RoleID { get; set; }
        public string? RoleName { get; set; }
        public string? IsActive { get; set; }
    }

    public class SMTPDefaultModel
    {
        public int ID { get; set; }
    }

}
