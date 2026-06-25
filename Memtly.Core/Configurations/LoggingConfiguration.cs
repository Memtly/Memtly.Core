using Memtly.Core.Constants;
using Memtly.Core.Helpers;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;

namespace Memtly.Core.Configurations
{
    public static class LoggingConfiguration
    {
        public static void AddLoggingConfiguration(this IServiceCollection services)
        {
            var bsp = services.BuildServiceProvider();
            var config = bsp.GetRequiredService<IConfiguration>();
            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext();

            var configHelper = bsp.GetRequiredService<IConfigHelper>();
            var settings = bsp.GetRequiredService<ISettingsHelper>();

            var enabled = settings.GetOrDefault(MemtlyConfiguration.Logging.Graylog.Enabled, true).Result;
            if (enabled)
            {
                var endpoint = configHelper.GetOrDefault(MemtlyConfiguration.Logging.Graylog.Endpoint, string.Empty);
                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    var transportType = EnumHelper.TryParse(configHelper.GetOrDefault(MemtlyConfiguration.Logging.Graylog.TransportType, "tcp"), TransportType.Tcp);
                    var logLevel = EnumHelper.TryParse(configHelper.GetOrDefault(MemtlyConfiguration.Logging.Graylog.TransportType, LogEventLevel.Error.ToString()), LogEventLevel.Error);
                    if (endpoint.Contains("memtly.com", StringComparison.OrdinalIgnoreCase))
                    {
                        logLevel = LogEventLevel.Error;
                    }

                    loggerConfig.WriteTo.Graylog(new GraylogSinkOptions
                    {
                        Facility = $"Memtly ({MemtlyCore.Version})",
                        HostnameOrAddress = endpoint,
                        Port = configHelper.GetOrDefault(MemtlyConfiguration.Logging.Graylog.Port, 443),
                        UseSsl = configHelper.GetOrDefault(MemtlyConfiguration.Logging.Graylog.UseSSL, true),
                        TransportType = transportType,
                        MinimumLogEventLevel = logLevel
                    });

                    Log.Logger = loggerConfig.CreateLogger();

                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddSerilog(Log.Logger, dispose: true);
                    });
                    services.AddSingleton(Log.Logger);
                }
            }
        }
    }
}