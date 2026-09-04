namespace RevenueBillingApp.Models.Base
{
    public class Action
    {
        public int? id { get; set; }
        public int workflowcode { get; set; }
        public string? actionname { get; set; }
        public int? workflowstepid { get; set; }
        public int? workflownextstepid { get; set; }
        public bool? isactive { get; set; }
        public DateTime? createddatetime { get; set; }
        public string? createdby { get; set; }
        public DateTime? editdatetime { get; set; }
        public string? editdby { get; set; }
        public bool ismove { get; set; }
        public bool issave { get; set; }
        public string actiontype { get; set; }
        public string assignmentType { get; set; }
        public string? nexttype { get; set; }
    }
}
