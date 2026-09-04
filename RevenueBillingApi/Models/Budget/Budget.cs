namespace RevenueBillingApi.Models.Budget
{
    public class Budget
    {
        public string workflow { get; set; }
        public string Department { get; set; }
        public string Purpose { get; set; }
        public int EstimatedAmount { get; set; }
        public DateTime RequiredBy { get; set; }
        public string Priority { get; set; }
        public int? CreatedBy { get; set; }
        public int? userid { get; set; }
        public int? instanceid { get; set; }
    }
}
