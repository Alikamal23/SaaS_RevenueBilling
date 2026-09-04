namespace RevenueBillingApp.Models.FileUpload
{
    public class FileUploadInfo
    {
        public string? FileTypes { get; set; }

        public IFormFile? CSVFile { get; set; }


        public List<IFormFile>? files { get; set; }
    }
}
