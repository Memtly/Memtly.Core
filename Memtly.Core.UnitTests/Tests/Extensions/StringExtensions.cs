using Memtly.Core.Extensions;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class StringExtensionsTests
    {
        public StringExtensionsTests()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("abcdefgh", new [] { "g" }, "+", "abcdef+h")]
        [TestCase("abcdefgh", new [] { "b", "e" }, "-", "a-cd-fgh")]
        [TestCase("abcdefgh", new [] { "bc", "e" }, "-", "a-d-fgh")]
        [TestCase("abcdefgeh", new [] { "bc", "e" }, "$", "a$d$fg$h")]
        [TestCase("abcdefgeh", new[] { "ee", "z" }, "$", "abcdefgeh")]
        public void StringExtensions_Replace(string input, string[] oldChars, string newChar, string expected)
        {
            var actual = input.Replace(oldChars, newChar);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("abcdefge", "c", "abdefge")]
        [TestCase("abcdefge", "cd", "abefge")]
        [TestCase("abcdefge", "def", "abcge")]
        [TestCase("abcdefge", "e", "abcdfg")]
        [TestCase("abcdefg", "g", "abcdef")]
        [TestCase("abcdefg", "a", "bcdefg")]
        public void StringExtensions_Remove(string input, string value, string expected)
        {
            var actual = input.Remove(value);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}