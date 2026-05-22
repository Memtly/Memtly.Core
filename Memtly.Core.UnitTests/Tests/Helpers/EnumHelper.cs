using Memtly.Core.Helpers;
using Memtly.Core.UnitTests.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class EnumHelperTests
    {
        public EnumHelperTests()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("none", MockEnum.Primary, MockEnum.None)]
        [TestCase("primary", MockEnum.None, MockEnum.Primary)]
        [TestCase("secondary", MockEnum.None, MockEnum.Secondary)]
        [TestCase("tertiary", MockEnum.None, MockEnum.None)]
        [TestCase("None", MockEnum.Primary, MockEnum.None)]
        [TestCase("Primary", MockEnum.None, MockEnum.Primary)]
        [TestCase("Secondary", MockEnum.None, MockEnum.Secondary)]
        [TestCase("Tertiary", MockEnum.None, MockEnum.None)]
        [TestCase("Tertiary", MockEnum.Primary, MockEnum.Primary)]
        [TestCase("Primary ", MockEnum.None, MockEnum.Primary)]
        [TestCase(" Primary", MockEnum.None, MockEnum.Primary)]
        [TestCase(" Primary ", MockEnum.None, MockEnum.Primary)]
        public void EnumHelper_TryParse(string value, MockEnum defaultValue, MockEnum expected)
        {
            var actual = EnumHelper.TryParse(value, defaultValue);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}