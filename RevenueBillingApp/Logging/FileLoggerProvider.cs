using Microsoft.Extensions.Options;

namespace RevenueBillingApp.Logging
{
    [ProviderAlias("FileLoggerProvider")]
    public class FileLoggerProvider : ILoggerProvider
    {
        public string _filepath = "APIlog_{Date}.log";
        public string _folderpath;

        private readonly IConfiguration _config;

        public FileLoggerProvider(IConfiguration config)
        {
            _config = config;
            _folderpath = Environment.GetEnvironmentVariable("AM_FOLDERPATH");

            if (!Directory.Exists(_folderpath))
            {
                Directory.CreateDirectory(_folderpath);

            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(this);
        }

        public void Dispose()
        {
        }
    }
}
