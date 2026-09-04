namespace RevenueBillingApi.Logging
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder FileLogger(this ILoggingBuilder builder,Action<FileLoggerOptions> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider,FileLoggerProvider>();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}
