namespace RevenueBillingApp.Models.FileUpload
{
    public class UserProfile
    {
        public string? UserID { get; set; }
        public string? UserName { get; set; }
        public string? FormID { get; set; }
        public IFormFile? _ImageFile { get; set; }
        public string? ImageFile { get; set; }
    }
}
