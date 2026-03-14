using Memtly.Core.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class GalleryHelperTests
    {
        public GalleryHelperTests()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("all", true)]
        [TestCase("default", true)]
        [TestCase("38be9fc63cf343929629a1aff93f2598", true)]
        [TestCase("All", false)]
        [TestCase("Default", false)]
        [TestCase("38Be9fc63cf343929629a1aff93f2598", false)]
        [TestCase("38be9f-c63cf343929629a1af-f93f2598", false)]
        public void GalleryHelper_IsValidGalleryIdentifier(string value, bool expected)
        {
            var actual = GalleryHelper.IsValidGalleryIdentifier(value);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase()]
        public void GalleryHelper_GenerateGalleryIdentifier()
        {
            var actual = GalleryHelper.IsValidGalleryIdentifier(GalleryHelper.GenerateGalleryIdentifier());
            Assert.That(actual, Is.EqualTo(true));
        }

        [TestCase()]
        public void GalleryHelper_GenerateGalleryIdentifier_Unique()
        {
            var identity1 = GalleryHelper.GenerateGalleryIdentifier();
            var identity2 = GalleryHelper.GenerateGalleryIdentifier();

            Assert.That(identity1, Is.Not.EqualTo(identity2));
        }
    }
}