using Memtly.Core.Constants;
using Memtly.Core.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class EncrytpionHelperTests
    {
        private readonly ISettingsHelper _settings = Substitute.For<ISettingsHelper>();

        public EncrytpionHelperTests()
        {
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.HashType, Arg.Any<string>()).Returns("SHA256");
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Iterations, Arg.Any<int>()).Returns(1000);
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("Test", "Key1", "Salt1", "ZMw15YpZ+uph9psdR6tEZg==")]
        [TestCase("Test", "Key2", "Salt2", "p/fwjLVXvJ2dRKbXDNhxDA==")]
        [TestCase("Test", "Key3", "Salt3", "47VYeotX2C8GPuhaQlrWXg==")]
        public void EncrytpionHelper_ValidDetails(string value, string key, string salt, string expected)
        {
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Key, Arg.Any<string>()).Returns(key);
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Salt, Arg.Any<string>()).Returns(salt);

            var actual = new EncryptionHelper(_settings).Encrypt(value);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Test1", "Salt1")]
        [TestCase("Test2", "Salt2")]
        [TestCase("Test3", "Salt3")]
        public void EncrytpionHelper_NoKey(string value, string salt)
        {
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Key, Arg.Any<string>()).Returns(string.Empty);
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Salt, Arg.Any<string>()).Returns(salt);

            var actual = new EncryptionHelper(_settings).Encrypt(value);
            Assert.That(actual, Is.EqualTo(value));
        }

        [TestCase("Test1", "Key1")]
        [TestCase("Test2", "Key2")]
        [TestCase("Test3", "Key3")]
        public void EncrytpionHelper_NoSalt(string value, string key)
        {
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Key, Arg.Any<string>()).Returns(key);
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Salt, Arg.Any<string>()).Returns(string.Empty);

            var actual = new EncryptionHelper(_settings).Encrypt(value);
            Assert.That(actual, Is.EqualTo(value));
        }

        [TestCase("Test1", "Key1", "Salt1")]
        [TestCase("Test2", "Key2", "Salt2")]
        [TestCase("Test3", "Key3", "Salt3")]
        public void EncrytpionHelper_DifferentHashes(string value, string key, string salt)
        {
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Key, Arg.Any<string>()).Returns(key);
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Salt, Arg.Any<string>()).Returns(salt);
            var helper1 = new EncryptionHelper(_settings).Encrypt(value);

            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Key, Arg.Any<string>()).Returns("Unit");
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Salt, Arg.Any<string>()).Returns("Test");
            var helper2 = new EncryptionHelper(_settings).Encrypt(value);

            Assert.That(helper1, Is.Not.EqualTo(helper2));
        }

        [TestCase("Key", "", false)]
        [TestCase("", "Salt", false)]
        [TestCase("Key", "Salt", true)]
        public void EncrytpionHelper_IsEncryptionEnabled(string key, string salt, bool expected)
        {
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Key, Arg.Any<string>()).Returns(key);
            _settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Salt, Arg.Any<string>()).Returns(salt);
            
            var actual = new EncryptionHelper(_settings).IsEncryptionEnabled();

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}