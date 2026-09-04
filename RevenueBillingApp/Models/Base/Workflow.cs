namespace RevenueBillingApp.Models.Base
{
    public class Workflow
    {
        public string? workflowcode { get; set; }
        public int? id { get; set; }
        public string? workflowname { get; set; }
        public bool? isactive { get; set; }
        public DateTime? createddatetime { get; set; }
        public string? createdby { get; set; }
        public DateTime? editdatetime { get; set; }
        public string? editdby { get; set; }
        public string? formpageurl { get; set; }
        public string? viewpageurl { get; set; }
        public string? taskpageurl { get; set; }
        public string? requestpageurl { get; set; }
    }
}
