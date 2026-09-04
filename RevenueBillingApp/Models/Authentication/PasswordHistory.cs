namespace RevenueBillingApp.Models.Authentication
{
    public class PasswordHistory
    {
        public int? HistoryID { get; set; }
        public int? UserID { get; set; }
        public string? UserPassword { get; set; }
        public DateTime? PasswordDate { get; set; }
    }
}
