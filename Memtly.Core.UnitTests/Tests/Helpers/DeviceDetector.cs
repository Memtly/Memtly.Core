using Memtly.Core.Enums;
using Memtly.Core.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class DeviceDetectorTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36", DeviceType.Desktop)]
        [TestCase("Mozilla/5.0 (Linux; Android 7.0; SM-T827R4 Build/NRD90M) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.116 Safari/537.36", DeviceType.Tablet)]
        [TestCase("Mozilla/5.0 (Linux; Android 13; SM-G998B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Mobile Safari/537.36", DeviceType.Mobile)]
        [TestCase("Googlebot-Image/1.0", DeviceType.Desktop)]
        public async Task DeviceDetector_ParseDeviceType(string userAgent, DeviceType expected)
        {
            var actual = await new DeviceDetector().ParseDeviceType(userAgent);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}