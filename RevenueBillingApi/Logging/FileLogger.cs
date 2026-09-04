using System.Diagnostics.CodeAnalysis;

namespace RevenueBillingApi.Logging
{
    public class FileLogger : ILogger
    {
        protected readonly FileLoggerProvider _fileLoggerProvider;
        private static readonly object _lock = new object();
        private static string _currentLogFilePath = null;
        private static DateTime _currentLogDate = DateTime.MinValue;

        public FileLogger([NotNull] FileLoggerProvider fileLoggerProvider)
        {
            _fileLoggerProvider = fileLoggerProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var currentDate = DateTime.UtcNow.Date;
            var logFilePath = GetLogFilePath(currentDate);

            var logRecord = $"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")} [{logLevel}], {formatter(state, exception)}{(exception != null ? Environment.NewLine + exception.StackTrace : "")}";

            lock (_lock)
            {
                try
                {
                    if (logFilePath != _currentLogFilePath)
                    {
                        _currentLogFilePath = logFilePath;
                        _currentLogDate = currentDate;
                    }

                    using (StreamWriter streamWriter = File.AppendText(_currentLogFilePath))
                    {
                        streamWriter.WriteLine(logRecord);
                    }
                }
                catch (Exception ex)
                {
                    // Handle or log any exceptions occurred during file writing
                    Console.WriteLine($"Error writing to log file: {ex.Message}");
                }
            }
        }

        private string GetLogFilePath(DateTime currentDate)
        {
            if (currentDate != _currentLogDate || string.IsNullOrEmpty(_currentLogFilePath))
            {
                var fileName = $"Applog_{currentDate.ToString("yyyy-MM-dd")}.txt";
                _currentLogFilePath = Path.Combine(_fileLoggerProvider._folderpath, fileName);
                _currentLogDate = currentDate;
            }

            return _currentLogFilePath;
        }
    }
}
