using Microsoft.Extensions.Configuration;

namespace Memtly.Core.UnitTests.Helpers
{
    internal class ConfigurationHelper
    {
        public static IConfiguration MockConfiguration(IDictionary<string, string?> settings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings ?? new Dictionary<string, string?>())
                .Build();
        }
    }
}