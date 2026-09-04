namespace RevenueBillingApi.Models.Workflow
{
    public class SectionPermission
    {
        public int sectionpermissionid { get; set; }
        public bool isvisible { get; set; }
        public bool isenable { get; set; }
        public string? editby { get; set; }
    }
}
