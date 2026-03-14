using Memtly.Core.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class EmailValidationHelperTests
    {
        public EmailValidationHelperTests()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("test", false)]
        [TestCase("test.unit", false)]
        [TestCase("test.unit@", false)]
        [TestCase("test.unit@com", false)]
        [TestCase("test.unit@unit@com", false)]
        [TestCase("test.unit@unit.com", true)]
        public void EmailValidationHelper_IsValid(string input, bool expected)
        {
            var actual = EmailValidationHelper.IsValid(input);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}