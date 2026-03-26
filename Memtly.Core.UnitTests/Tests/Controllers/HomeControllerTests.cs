using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Memtly.Core.Constants;
using Memtly.Core.Controllers;
using Memtly.Core.Enums;
using Memtly.Core.Helpers;
using Memtly.Core.Helpers.Database;
using Memtly.Core.Models.Database;
using Memtly.Core.UnitTests.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class HomeControllerTests
    {
        private readonly ISettingsHelper _settings = Substitute.For<ISettingsHelper>();
        private readonly IDatabaseHelper _database = Substitute.For<IDatabaseHelper>();
        private readonly IDeviceDetector _deviceDetector = Substitute.For<IDeviceDetector>();
        private readonly IAuditHelper _audit = Substitute.For<IAuditHelper>();
        private readonly ILogger<HomeController> _logger = Substitute.For<ILogger<HomeController>>();
        private readonly IStringLocalizer<Memtly.Localization.Translations> _localizer = Substitute.For<IStringLocalizer<Memtly.Localization.Translations>>();
        
        public HomeControllerTests()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase(DeviceType.Desktop, true, "", true)]
        [TestCase(DeviceType.Desktop, false, "", false)]
        [TestCase(DeviceType.Mobile, true, "", true)]
        [TestCase(DeviceType.Mobile, false, "", false)]
        [TestCase(DeviceType.Desktop, true, "123456", false)]
        [TestCase(DeviceType.Desktop, false, "Abc123!", false)]
        [TestCase(DeviceType.Mobile, true, "abc123!", false)]
        [TestCase(DeviceType.Mobile, false, "adsbsds", false)]
        public async Task HomeController_Index(DeviceType deviceType, bool singleGalleryMode, string secretKey, bool isRedirect)
        {
            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(deviceType);
            _database.GetGallery(1).Returns(new GalleryModel() 
            {
                SecretKey = secretKey,
            });
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(singleGalleryMode);

            var controller = new HomeController(_settings, _database, _deviceDetector, _audit, _logger, _localizer);
            controller.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = new MockSession()
            };

            if (!isRedirect)
            {
                ViewResult actual = (ViewResult)await controller.Index();
                Assert.That(actual, Is.TypeOf<ViewResult>());
            }
            else
            { 
                RedirectToActionResult actual = (RedirectToActionResult)await controller.Index();
                Assert.That(actual, Is.TypeOf<RedirectToActionResult>());
                Assert.That(actual.Permanent, Is.EqualTo(false));
                Assert.That(actual.ControllerName, Is.EqualTo("Gallery"));
                Assert.That(actual.ActionName, Is.EqualTo("Index"));
                Assert.That(actual.RouteValues, singleGalleryMode ? Is.EqualTo(new RouteValueDictionary { { "identifier", "default" } }) : Is.Null);
                Assert.That(actual.Fragment, Is.Null);
            }
        }
    }
}