using System.Globalization;
using Memtly.Core.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class LanguageHelperTests
    {
        private readonly List<CultureInfo> _supportedCultures;

        public LanguageHelperTests()
        {
            _supportedCultures = new List<CultureInfo>()
            {
                new CultureInfo("en-GB"),
                new CultureInfo("fr-FR"),
                new CultureInfo("de-DE"),
            };
        }

        [SetUp]
        public void Setup()
        {            
        }

        [TestCase("en-GB", true)]
        [TestCase("fr-FR", true)]
        [TestCase("de-DE", true)]
        [TestCase("de-de", true)]
        [TestCase("en-US", false)]
        [TestCase("cn-CSS", false)]
        [TestCase("", false)]
        public void LanguageHelper_IsCultureSupported(string culture, bool expected)
        {
            var actual = new LanguageHelper().IsCultureSupported(culture, _supportedCultures);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("en-GB", "en-GB", "en-GB")]
        [TestCase("en-US", "en-GB", "en-GB")]
        [TestCase("en-US", "en-US", "en-GB")]
        [TestCase("fr-FR", "en-GB", "fr-FR")]
        [TestCase("de-DE", "fr-FR", "de-DE")]
        [TestCase("de-de", "fr-fr", "de-DE")]
        public void LanguageHelper_GetOrFallbackCulture(string culture, string fallback, string expected)
        {
            var actual = new LanguageHelper().GetOrFallbackCulture(culture, fallback, _supportedCultures);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}