using Memtly.Core.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class PasswordHelperTests
    {
        public PasswordHelperTests()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("Password1!", true)]
        [TestCase("Password1", false)]
        [TestCase("Password!", false)]
        [TestCase("password1!", false)]
        [TestCase("PASSWORD1!", false)]
        [TestCase("Pass1!", false)]
        public void PasswordHelper_IsValid(string input, bool expected)
        {
            var actual = PasswordHelper.IsValid(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Password1!", true)]
        [TestCase("Admin1!", true)]
        [TestCase("Q*hgJ8FcSkm9$q7B9Zf#T7*LJ5", false)]
        public void PasswordHelper_IsWeak(string input, bool expected)
        {
            var actual = PasswordHelper.IsWeak(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(true, true, true, true, 30, true)]
        [TestCase(true, true, true, true, 10, true)]
        [TestCase(true, true, true, true, 5, false)]
        [TestCase(false, true, true, true, 30, false)]
        [TestCase(true, false, true, true, 30, false)]
        [TestCase(true, true, false, true, 30, false)]
        [TestCase(true, true, true, false, 30, false)]
        [TestCase(false, false, false, false, 30, false)]
        public void PasswordHelper_GenerateTempPassword(bool lower, bool upper, bool numbers, bool symbols, int length, bool isStrong)
        {
            var password = PasswordHelper.GenerateTempPassword(lower: lower, upper: upper, numbers: numbers, symbols: symbols, length: length);
            var actual = PasswordHelper.IsValid(password) && !PasswordHelper.IsWeak(password);
            Assert.That(actual, Is.EqualTo(isStrong), $"Password: '{password}' is not valid");
        }

        [TestCase()]
        public void PasswordHelper_GenerateGallerySecretKey()
        {
            var actual = PasswordHelper.GenerateGallerySecretKey();
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Length, Is.AtLeast(30));
        }

        [TestCase()]
        public void PasswordHelper_GenerateSecretCode()
        {
            var actual = PasswordHelper.GenerateSecretCode();
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Length, Is.AtLeast(20));
        }

        [TestCase()]
        public void PasswordHelper_GenerateSecretCode_IsDifferent()
        {
            var actual1 = PasswordHelper.GenerateSecretCode();
            var actual2 = PasswordHelper.GenerateSecretCode();
            Assert.That(actual1, Is.Not.EqualTo(actual2));
        }
    }
}