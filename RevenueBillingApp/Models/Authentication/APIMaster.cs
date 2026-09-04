namespace RevenueBillingApp.Models.Authentication
{
    public class APIMaster
    {
        public int ApiID { get; set; }
        public bool IsAllowed { get; set; }
        public string? ApiController { get; set; }
        public string? ApiAction { get; set; }
    }
}
