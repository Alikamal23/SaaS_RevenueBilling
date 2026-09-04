namespace RevenueBillingApi.Models.Workflow
{
    public class WorkflowMove
    {
        public int? instanceid { get; set; }
        public int? actionid { get; set; }
        public int? userid { get; set; }
        public string? comment { get; set; }
        public string? dynamicfunction { get; set; }
        public string? assignmenttype { get; set; }

    }
}
