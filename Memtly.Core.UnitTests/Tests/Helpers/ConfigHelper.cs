using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Memtly.Core.Helpers;
using Memtly.Core.UnitTests.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class ConfigHelperTests
    {
        private readonly IConfiguration _configuration;
        private readonly IEnvironmentWrapper _environment = Substitute.For<IEnvironmentWrapper>();
        private readonly ILogger<ConfigHelper> _logger = Substitute.For<ILogger<ConfigHelper>>();

        public ConfigHelperTests() 
        {
            _environment.GetEnvironmentVariable("VERSION").Returns("v2.0.0");
            _environment.GetEnvironmentVariable("ENVKEY_1").Returns("EnvValue1");
            _environment.GetEnvironmentVariable("ENVKEY_2").Returns("EnvValue2");
            _environment.GetEnvironmentVariable("ENVKEY_3").Returns("EnvValue3");

            _configuration = ConfigurationHelper.MockConfiguration(new Dictionary<string, string?>()
            {
                { "Release:Version", "v1.0.0" },
                { "Release:Plugin:Version", "v3.0.0" },

                { "String1:Key1", "Value1" },
                { "String1:Key2", "Value2" },
                { "String2:Key1", "Value3" },

                { "Int1:Key1", "1" },
                { "Int1:Key2", "2" },
                { "Int2:Key1", "3" },

                { "Long1:Key1", "4" },
                { "Long1:Key2", "5" },
                { "Long2:Key1", "6" },

                { "Decimal1:Key1", "4.12" },
                { "Decimal1:Key2", "5.45" },
                { "Decimal2:Key1", "6.733" },

                { "Double1:Key1", "4.12" },
                { "Double1:Key2", "5.45" },
                { "Double2:Key1", "6.733" },

                { "Boolean1:Key1", "true" },
                { "Boolean1:Key2", "false" },
                { "Boolean2:Key1", "true" },

                { "DateTime1:Key1", "1987-11-20 08:00:00" },
                { "DateTime1:Key2", "2000-08-12 12:00:00" },
                { "DateTime2:Key1", "2018-01-01 20:30:10" },
            });
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("Memtly:ENVKEY:1", "EnvValue1")]
        [TestCase("Memtly:ENVKEY:2", "EnvValue2")]
        [TestCase("Memtly:ENVKEY:3", "EnvValue3")]
        [TestCase("Memtly:ENVKEY:4", null)]
        [TestCase("RELEASE:VERSION", null)]
        public void ConfigHelper_GetEnvironmentVariable(string section, string? expected)
        {
            var actual = new ConfigHelper(_environment, _configuration, _logger).GetEnvironmentVariable(section);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("String1:Key1", "Value1")]
        [TestCase("String1:Key2", "Value2")]
        [TestCase("String2:Key1", "Value3")]
        [TestCase("String2:Key2", null)]
        [TestCase("Release:Version", "v1.0.0")]
        public void ConfigHelper_GetConfigValue(string key, string? expected)
        {
            var actual = new ConfigHelper(_environment, _configuration, _logger).GetConfigValue(key);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("String1:Key1", "Value1")]
        [TestCase("String1:Key2", "Value2")]
        [TestCase("String2:Key1", "Value3")]
        [TestCase("String2:Key2", null)]
        [TestCase("Release:Version", "v1.0.0")]
        public void ConfigHelper_Get(string key, string? expected)
        {
            var actual = new ConfigHelper(_environment, _configuration, _logger).Get(key);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("String1:Key1", "Default", "Value1")]
        [TestCase("String1:Key2", "Default", "Value2")]
        [TestCase("String2:Key1", "Default", "Value3")]
        [TestCase("String2:Key2", "Default", "Default")]
        [TestCase("Release:Version", "v0.0.0", "v1.0.0")]
        [TestCase("Release:Plugin:Version", "v0.0.0", "v3.0.0")]
        public void ConfigHelper_GetOrDefault(string key, string defaultValue, string expected)
        {
            var actual = new ConfigHelper(_environment, _configuration, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Int1:Key1", 999, 1)]
        [TestCase("Int1:Key2", 999, 2)]
        [TestCase("Int2:Key1", 999, 3)]
        [TestCase("Int2:Key2", 999, 999)]
        public void ConfigHelper_GetOrDefault(string key, int defaultValue, int expected)
        {
            var actual = new ConfigHelper(_environment, _configuration, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Long1:Key1", 999, 4)]
        [TestCase("Long1:Key2", 999, 5)]
        [TestCase("Long2:Key1", 999, 6)]
        [TestCase("Long2:Key2", 999, 999)]
        public void ConfigHelper_GetOrDefault(string key, long defaultValue, long expected)
        {
            var actual = new ConfigHelper(_environment, _configuration, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Decimal1:Key1", 999, 4.12)]
        [TestCase("Decimal1:Key2", 999, 5.45)]
        [TestCase("Decimal2:Key1", 999, 6.733)]
        [TestCase("Decimal2:Key2", 999, 999)]
        public void ConfigHelper_GetOrDefault(string key, decimal defaultValue, decimal expected)
        {
            var actual = new ConfigHelper(_environment, _configuration, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Double1:Key1", 999, 4.12)]
        [TestCase("Double1:Key2", 999, 5.45)]
        [TestCase("Double2:Key1", 999, 6.733)]
        [TestCase("Double2:Key2", 999, 999)]
        public void ConfigHelper_GetOrDefault(string key, double defaultValue, double expected)
        {
            var actual = new ConfigHelper(_environment, _configuration, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Boolean1:Key1", false, true)]
        [TestCase("Boolean1:Key2", false, false)]
        [TestCase("Boolean2:Key1", false, true)]
        [TestCase("Boolean2:Key2", true, true)]
        public void ConfigHelper_GetOrDefault(string key, bool defaultValue, bool expected)
        {
            var actual = new ConfigHelper(_environment, _configuration, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("DateTime1:Key1", null, "1987-11-20 08:00:00")]
        [TestCase("DateTime1:Key2", null, "2000-08-12 12:00:00")]
        [TestCase("DateTime2:Key1", null, "2018-01-01 20:30:10")]
        [TestCase("DateTime2:Key2", null, null)]
        [TestCase("DateTime3:Key3", "2350-05-05 00:05:12", "2350-05-05 00:05:12")]
        public void ConfigHelper_GetOrDefault(string key, DateTime? defaultValue, string? expected)
        {
            var actual = new ConfigHelper(_environment, _configuration, _logger).GetOrDefault(key, defaultValue);
            Assert.That(actual, Is.EqualTo(!string.IsNullOrWhiteSpace(expected) ? DateTime.Parse(expected) : null));
        }
    }
}