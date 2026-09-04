namespace RevenueBillingApp.Models.Authentication
{
    public class Tokens
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public DateTime start_date_time { get; set; }
        public DateTime end_date_time { get; set; }
    }
}
