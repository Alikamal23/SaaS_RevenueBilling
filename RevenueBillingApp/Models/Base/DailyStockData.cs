namespace RevenueBillingApp.Models.Base
{
    public class DailyStockData
    {
        public string? DSID { get; set; }
        public string? CompanyID { get; set; }
        public string? Open { get; set; }
        public string? High { get; set; }
        public string? Low { get; set; }
        public string? Close { get; set; }
        public string? PreviousClose { get; set; }
        public string? Totalvolume { get; set; }
        public string? FreeFloat { get; set; }
        public string? OutstandingShares { get; set; }
        public string? FloatSemiAnnual { get; set; }
        public string? XPrice { get; set; }
        public string? DSDate { get; set; }
    }
}
