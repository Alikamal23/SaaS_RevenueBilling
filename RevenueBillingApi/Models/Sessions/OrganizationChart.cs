namespace RevenueBillingApi.Models.Settings
{
    public class OrganizationChart
    {
        public string? organizationcode { get; set; }
        public string? organizationname { get; set; }
        public int managerid { get; set; }
        public string[] employeeid { get; set; }
        public bool isactive { get; set; }
        public int? createdby { get; set; }
        public bool isedit { get; set; }

    }
}
