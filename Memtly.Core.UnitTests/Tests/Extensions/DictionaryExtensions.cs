using Memtly.Core.Extensions;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class DictionaryExtensionsTests
    {
        private readonly IDictionary<string, string> _data;

        public DictionaryExtensionsTests()
        {
            _data = new Dictionary<string, string>()
            {
                { "KEY_1", "1" },
                { "KEY_2", "2" },
                { "KEY_3", "3" },
                { "KEY_4", "4" },
            };
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("KEY_1", "1")]
        [TestCase("KEY_2", "2")]
        [TestCase("KEY_3", "3")]
        [TestCase("KEY_99", "")]
        public void DictionaryExtensions_GetValue(string key, string expected)
        {
            var actual = _data.GetValue(key);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("KEY_1", "Default", "1")]
        [TestCase("KEY_2", "Default", "2")]
        [TestCase("KEY_3", "Default", "3")]
        [TestCase("KEY_99", "Default", "Default")]
        public void DictionaryExtensions_GetValue_DefaultValue(string key, string defaultValue, string? expected)
        {
            var actual = _data.GetValue(key, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}