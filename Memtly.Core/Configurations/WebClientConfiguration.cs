using Memtly.Core.Constants;
using Memtly.Core.Helpers;

namespace Memtly.Core.Configurations
{
    public static class WebClientConfiguration
    {
        public static void AddWebClientConfiguration(this IServiceCollection services)
        {
            var bsp = services.BuildServiceProvider();

            services.AddHttpClient("SponsorsClient", (client) =>
            {
                var config = bsp.GetRequiredService<IConfigHelper>();
                client.BaseAddress = new Uri(config.GetOrDefault(MemtlyConfiguration.Sponsors.Url, "http://localhost:5000/"));
                client.Timeout = TimeSpan.FromSeconds(5);
            });
        }
    }
}