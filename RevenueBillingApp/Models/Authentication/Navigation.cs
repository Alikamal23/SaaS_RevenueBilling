namespace RevenueBillingApp.Models.Authentication
{
    public class Navigation
    {
        public int FormID { get; set; }
        public string? FormController { get; set; }
        public string? FormAction { get; set; }
        public string? FormDisplayName { get; set; }
        public string? iconclass { get; set; }
        public bool IsActive { get; set; }
        public int ParentFormID { get; set; }
    }

   
}
