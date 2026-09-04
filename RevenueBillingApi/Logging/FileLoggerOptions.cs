namespace RevenueBillingApi.Logging
{
    public class FileLoggerOptions
    {
        private string _filepath = "APIlog_{Date}.log";
        private string _folderpath;
        private readonly IConfiguration _config;
        public FileLoggerOptions(IConfiguration config)
        {
            _config = config;
            _folderpath = Environment.GetEnvironmentVariable("AM_FOLDERPATH");
        }
        public virtual string FilePath
        {
            get { return _filepath; }
        }
        public virtual string FolderPath
        {
            get { return _folderpath; }
        }
    }
}
