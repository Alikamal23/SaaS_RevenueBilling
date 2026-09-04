namespace RevenueBillingApp.Models.Authentication
{
    public class RolesMapping
    {
        public string RoleName { get; set; }
        public int RoleID { get; set; }
        public string FormName { get; set; }
        public string FormDisplayName { get; set; }
        public int FormID { get; set; }
        public int? FormType { get; set; }
        public string GroupName { get; set; }
        public bool IsView { get; set; }
        public bool AllowInsert { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
        public string JSONData { get; set; }
        public List<APIMaster> ApiMaster { get; set; }

    }
}