namespace RevenueBillingApp.Models.Base
{
    public class Step
    {
        public int? id { get; set; }
        public int? workflowid { get; set; }
        public string? workflowstep { get; set; }
        public int? RoleID { get; set; }
        public string? RoleName { get; set; }
        public int? sla { get; set; }
        public int? approvaltypeid { get; set; }
        public string? approvaltype { get; set; }
        public int? sortid {  get; set; }
        public bool? isactive { get; set; }
        public DateTime? createddatetime { get; set; }
        public string? createdby { get; set; }
        public DateTime? editdatetime { get; set; }
        public string? editdby { get; set; }
    }
}
