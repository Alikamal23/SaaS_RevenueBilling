namespace RevenueBillingApp.Models.Authentication
{
    public class ErrorInfo
    {
        public int? ID { get; set; }
        public string? errorname { get; set; }
        public string? errormessage { get; set; }
        public bool? isactive { get; set; }
        public DateTime? createdate { get; set; }
    }
}
