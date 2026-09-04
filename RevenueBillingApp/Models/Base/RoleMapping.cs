namespace RevenueBillingApp.Models.Base
{
    public class RoleMapping
    {
        public string? RoleID { get; set; }
        public string? UserID { get; set; }
        public List<RoleMappingCollection>? RoleMappingCollection { get; set; }
    }
}
