using Microsoft.Extensions.Logging;
using Memtly.Core.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class FileHelperTests
    {
        private readonly ILogger<FileHelper> _logger = Substitute.For<ILogger<FileHelper>>();

        public FileHelperTests()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase(-1, "0 B")]
        [TestCase(0, "0 B")]
        [TestCase(1, "1 B")]
        [TestCase(2, "2 B")]
        [TestCase(3, "3 B")]
        public void FileHelper_BytesToHumanReadable(long bytes, string expected)
        {
            var actual = new FileHelper(_logger).BytesToHumanReadable(bytes, 0);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(-1, "0 B")]
        [TestCase(0, "0 B")]
        [TestCase(1, "1 B")]
        [TestCase(10, "10 B")]
        [TestCase(100, "100 B")]
        [TestCase(1000, "1 KB")]
        [TestCase(1000000, "1 MB")]
        [TestCase(1000000000, "1 GB")]
        [TestCase(1000000000000, "1 TB")]
        [TestCase(1000000000000000, "1 PB")]
        [TestCase(1000000000000000000, "1 EB")]
        public void FileHelper_BytesToHumanReadable_No_Places(long bytes, string expected)
        {
            var actual = new FileHelper(_logger).BytesToHumanReadable(bytes, 0);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(-1, "0.0 B")]
        [TestCase(0, "0.0 B")]
        [TestCase(1, "1.0 B")]
        [TestCase(10, "10.0 B")]
        [TestCase(100, "100.0 B")]
        [TestCase(1000, "1.0 KB")]
        [TestCase(1000000, "1.0 MB")]
        [TestCase(1000000000, "1.0 GB")]
        [TestCase(1000000000000, "1.0 TB")]
        [TestCase(1000000000000000, "1.0 PB")]
        [TestCase(1000000000000000000, "1.0 EB")]
        public void FileHelper_BytesToHumanReadable_1_Places(long bytes, string expected)
        {
            var actual = new FileHelper(_logger).BytesToHumanReadable(bytes, 1);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(-1, "0.00 B")]
        [TestCase(0, "0.00 B")]
        [TestCase(1, "1.00 B")]
        [TestCase(10, "10.00 B")]
        [TestCase(100, "100.00 B")]
        [TestCase(1000, "1.00 KB")]
        [TestCase(1000000, "1.00 MB")]
        [TestCase(1000000000, "1.00 GB")]
        [TestCase(1000000000000, "1.00 TB")]
        [TestCase(1000000000000000, "1.00 PB")]
        [TestCase(1000000000000000000, "1.00 EB")]
        public void FileHelper_BytesToHumanReadable_2_Places(long bytes, string expected)
        {
            var actual = new FileHelper(_logger).BytesToHumanReadable(bytes, 2);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}