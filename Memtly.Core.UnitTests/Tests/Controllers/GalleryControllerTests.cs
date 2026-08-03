using System.Security.Claims;
using Memtly.Core.Constants;
using Memtly.Core.Controllers;
using Memtly.Core.Enums;
using Memtly.Core.Helpers;
using Memtly.Core.Helpers.Database;
using Memtly.Core.Helpers.Notifications;
using Memtly.Core.Models;
using Memtly.Core.Models.Database;
using Memtly.Core.UnitTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute.ReturnsExtensions;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class GalleryControllerTests
    {
        private readonly ISettingsHelper _settings = Substitute.For<ISettingsHelper>();
        private readonly IDatabaseHelper _database = Substitute.For<IDatabaseHelper>();
        private readonly IFileHelper _file = Substitute.For<IFileHelper>();
        private readonly IDeviceDetector _deviceDetector = Substitute.For<IDeviceDetector>();
        private readonly IImageHelper _image = Substitute.For<IImageHelper>();
        private readonly INotificationHelper _notification = Substitute.For<INotificationHelper>();
        private readonly IEncryptionHelper _encryption = Substitute.For<IEncryptionHelper>();
        private readonly Memtly.Core.Helpers.IUrlHelper _url = Substitute.For<Memtly.Core.Helpers.IUrlHelper>();
        private readonly IIdentityHelper _identity = Substitute.For<IIdentityHelper>();
        private readonly ILogger<GalleryController> _logger = Substitute.For<ILogger<GalleryController>>();
        private readonly IStringLocalizer<Memtly.Localization.Translations> _localizer = Substitute.For<IStringLocalizer<Memtly.Localization.Translations>>();

        public GalleryControllerTests()
        {
        }

        [SetUp]
        public void Setup()
        {
            var mockData = GetMockGalleryData();

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(1);
            _identity.GetUserPermissions(Arg.Any<ClaimsPrincipal>()).Returns(new Permissions());

            _database.GetGallery(1).Returns(Task.FromResult<GalleryModel?>(mockData["default"]));
            _database.GetGallery(2).Returns(Task.FromResult<GalleryModel?>(mockData["blaa"]));
            _database.GetGallery(3).Returns(Task.FromResult<GalleryModel?>(mockData["drop_test"]));
            _database.GetGallery(4).Returns(Task.FromResult<GalleryModel?>(null));

            _database.GetGalleryId("default").Returns(Task.FromResult<int?>(mockData["default"].Id));
            _database.GetGalleryId("blaa").Returns(Task.FromResult<int?>(mockData["blaa"].Id));
            _database.GetGalleryId("drop_test").Returns(Task.FromResult<int?>(mockData["drop_test"].Id));
            _database.GetGalleryId("missing").Returns(Task.FromResult<int?>(null));

            _database.GetGalleryIdByName("default").Returns(Task.FromResult<int?>(mockData["default"].Id));
            _database.GetGalleryIdByName("blaa").Returns(Task.FromResult<int?>(mockData["blaa"].Id));
            _database.GetGalleryIdByName("drop_test").Returns(Task.FromResult<int?>(mockData["drop_test"].Id));
            _database.GetGalleryIdByName("missing").Returns(Task.FromResult<int?>(null));

            _database.AddGallery(Arg.Any<GalleryModel>()).Returns(Task.FromResult<GalleryModel?>(new GalleryModel()
            {
                Id = 101,
                Name = "missing",
                SecretKey = "123456",
                ApprovedItems = 0,
                PendingItems = 0,
                TotalItems = 0,
                Owner = 1
            }));
            _database.AddGalleryItem(Arg.Any<GalleryItemModel>()).Returns(Task.FromResult<GalleryItemModel?>(MockData.MockGalleryItem()));

            _database.GetGalleryItems(Arg.Any<int>(), Arg.Any<int>(), GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(MockData.MockGalleryItems(10, 1, GalleryItemState.All)));
            _database.GetGalleryItems(Arg.Any<int>(), Arg.Any<int>(), GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(MockData.MockGalleryItems(10, 1, GalleryItemState.Pending)));
            _database.GetGalleryItems(Arg.Any<int>(), Arg.Any<int>(), GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(MockData.MockGalleryItems(10, 1, GalleryItemState.Approved)));
            _database.GetGalleryItemByChecksum(Arg.Any<int>(), Arg.Any<string>()).ReturnsNull();

            _settings.GetOrDefault(MemtlyConfiguration.Gallery.Upload, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.Download, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.UploadPeriod, Arg.Any<string>(), Arg.Any<int>()).Returns("1970-01-01 00:00:00");
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.PreventDuplicates, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.DefaultView, Arg.Any<int>(), Arg.Any<int>()).Returns((int)ViewMode.Default);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.AllowedFileTypes, Arg.Any<string>(), Arg.Any<int>()).Returns(".jpg,.jpeg,.png,.mp4,.mov");
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.RequireReview, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.MaxFileSizeMB, Arg.Any<int>(), Arg.Any<int>()).Returns(10);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(false);

            _file.GetChecksum(Arg.Any<string>()).Returns(Guid.NewGuid().ToString());

            _notification.Send(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(true));

            _localizer[Arg.Any<string>()].Returns(new LocalizedString("UnitTest", "UnitTest"));
        }

        [TestCase(DeviceType.Desktop, 1, "default", "default", "password", ViewMode.Default, GalleryGroup.None, GalleryOrder.Descending, true)]
        [TestCase(DeviceType.Mobile, 2, "blaa", "blaa", "456789", ViewMode.Presentation, GalleryGroup.DateUploaded, GalleryOrder.Ascending, true)]
        [TestCase(DeviceType.Mobile, 2, "blaa", "blaa", "456789", ViewMode.Presentation, GalleryGroup.DateTaken, GalleryOrder.Ascending, true)]
        [TestCase(DeviceType.Tablet, 101, "missing", "missing", "123456", ViewMode.Slideshow, GalleryGroup.Uploader, GalleryOrder.Ascending, false)]
        public async Task GalleryController_Index(DeviceType deviceType, int id, string? identifier, string? name, string? key, ViewMode? mode, GalleryGroup group, GalleryOrder order, bool existing)
        {
            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(deviceType);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(false);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.GuestGalleryCreation, Arg.Any<bool>()).Returns(false);

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            if (existing)
            {
                ViewResult actual = (ViewResult)await controller.Index(identifier, key, mode, group, order);
                Assert.That(actual, Is.TypeOf<ViewResult>());
                Assert.That(actual?.Model, Is.Not.Null);

                PhotoGallery model = (PhotoGallery)actual.Model;
                Assert.That(model?.Gallery?.Id, Is.EqualTo(id));
                Assert.That(model?.Gallery?.Identifier, Is.EqualTo(identifier));
                Assert.That(model?.Gallery?.Name, Is.EqualTo(name));
                Assert.That(model?.SecretKey, Is.EqualTo(key));
                Assert.That(model.ViewMode, Is.EqualTo(mode));
            }
            else
            {
                RedirectToActionResult actual = (RedirectToActionResult)await controller.Index(identifier, key, mode, group, order);
                Assert.That(actual, Is.TypeOf<RedirectToActionResult>());
            }
        }

        [TestCase("default", "default")]
        [TestCase("Default", "default")]
        [TestCase(null, null)]
        [TestCase("blaa", "blaa")]
        public async Task GalleryController_Index_GetByIdentifier(string? identifier, string? expected)
        {
            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(false);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.GuestGalleryCreation, Arg.Any<bool>()).Returns(false);

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            if (expected != null)
            {
                ViewResult actual = (ViewResult)await controller.Index(identifier, "password", ViewMode.Default, GalleryGroup.None, GalleryOrder.Random);
                Assert.That(actual, Is.TypeOf<ViewResult>());
                Assert.That(actual?.Model, Is.Not.Null);

                PhotoGallery model = (PhotoGallery)actual.Model;
                Assert.That(model?.Gallery?.Identifier, Is.EqualTo(expected));
            }
            else
            {
                RedirectToActionResult actual = (RedirectToActionResult)await controller.Index(identifier, "password", ViewMode.Default, GalleryGroup.None, GalleryOrder.Random);
                Assert.That(actual, Is.TypeOf<RedirectToActionResult>());
            }
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task GalleryController_UploadDisabled(bool enabled, bool expected)
        {
            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(false);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.Upload, Arg.Any<bool>(), Arg.Any<int>()).Returns(enabled);

            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(false);

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index("default", "password", ViewMode.Default, GalleryGroup.None, GalleryOrder.Descending);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.UploadActivated, Is.EqualTo(expected));
        }

        [TestCase("1970-01-01 00:00", true)]
        [TestCase("3000-01-01 00:00", false)]
        [TestCase("1970-01-01 00:00 / 1980-01-01 00:00", false)]
        [TestCase("2999-01-01 00:00 / 3000-01-01 00:00", false)]
        [TestCase("1970-01-01 00:00 / 3000-01-01 00:00", true)]
        public async Task GalleryController_UploadDisabled(string uploadPeriod, bool expected)
        {
            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(false);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.UploadPeriod, Arg.Any<string>(), Arg.Any<int>()).Returns(uploadPeriod);

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index("default", "password", ViewMode.Default, GalleryGroup.None, GalleryOrder.Descending);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.UploadActivated, Is.EqualTo(expected));
        }

        [TestCase(DeviceType.Desktop, ViewMode.Default, GalleryGroup.None, GalleryOrder.Descending)]
        [TestCase(DeviceType.Mobile, ViewMode.Presentation, GalleryGroup.DateUploaded, GalleryOrder.Ascending)]
        [TestCase(DeviceType.Mobile, ViewMode.Presentation, GalleryGroup.DateTaken, GalleryOrder.Ascending)]
        [TestCase(DeviceType.Tablet, ViewMode.Slideshow, GalleryGroup.Uploader, GalleryOrder.Ascending)]
        public async Task GalleryController_Index_SingleGalleryMode(DeviceType deviceType, ViewMode? mode, GalleryGroup group, GalleryOrder order)
        {
            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(deviceType);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index("default", "password", mode, group, order);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(1));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo("default"));
            Assert.That(model?.Gallery?.Name, Is.EqualTo("default"));
            Assert.That(model?.SecretKey, Is.EqualTo("password"));
            Assert.That(model.ViewMode, Is.EqualTo(mode));
        }

        [TestCase(GalleryType.Basic, 5)]
        [TestCase(GalleryType.Drop, 1)]
        [TestCase(GalleryType.Collection, 5)]
        public async Task GalleryController_Index_BasicUsers_ShowPendingUploads_Disabled(GalleryType type, int expectedItemCount)
        {
            var mockGallery = GetMockGalleryData().FirstOrDefault(x => x.Value.Type == type).Value;
            mockGallery.Owner = 1;

            var mockApprovedItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Approved, "jpg");
            var mockPendingItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Pending, "jpg");

            var mockItems = new List<GalleryItemModel>();
            mockItems.AddRange(mockApprovedItems);
            mockItems.AddRange(mockPendingItems);

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(2);
            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(false);

            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(false);

            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));

            _database.GetGalleryIdentifier(Arg.Any<int>()).Returns(new GalleryIdentifierModel() { Id = mockGallery.Id, Identifier = mockGallery.Identifier, Name = mockGallery.Name });

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(mockGallery.Identifier, mockGallery.SecretKey);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(mockGallery.Id));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(mockGallery.Identifier));
            Assert.That(model?.Gallery?.Name, Is.EqualTo(mockGallery.Name));
            Assert.That(model?.Images, Is.Not.Null);
            Assert.That(model.Images.Count, Is.EqualTo(expectedItemCount));
        }

        [TestCase(GalleryType.Basic, 5)]
        [TestCase(GalleryType.Drop, 5)]
        [TestCase(GalleryType.Collection, 5)]
        public async Task GalleryController_Index_PrivilegedUsers_ShowPendingUploads_Disabled(GalleryType type, int expectedItemCount)
        {
            var mockGallery = GetMockGalleryData().FirstOrDefault(x => x.Value.Type == type).Value;
            mockGallery.Owner = 1;

            var mockApprovedItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Approved, "jpg");
            var mockPendingItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Pending, "jpg");

            var mockItems = new List<GalleryItemModel>();
            mockItems.AddRange(mockApprovedItems);
            mockItems.AddRange(mockPendingItems);

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(2);
            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(false);

            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(false);

            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));

            _database.GetGalleryIdentifier(Arg.Any<int>()).Returns(new GalleryIdentifierModel() { Id = mockGallery.Id, Identifier = mockGallery.Identifier, Name = mockGallery.Name });

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(mockGallery.Identifier, mockGallery.SecretKey);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(mockGallery.Id));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(mockGallery.Identifier));
            Assert.That(model?.Gallery?.Name, Is.EqualTo(mockGallery.Name));
            Assert.That(model?.Images, Is.Not.Null);
            Assert.That(model.Images.Count, Is.EqualTo(expectedItemCount));
        }

        [TestCase(GalleryType.Basic, 6)]
        [TestCase(GalleryType.Drop, 2)]
        [TestCase(GalleryType.Collection, 6)]
        public async Task GalleryController_Index_BasicUsers_ShowPendingUploads_Enabled(GalleryType type, int expectedItemCount)
        {
            var mockGallery = GetMockGalleryData().FirstOrDefault(x => x.Value.Type == type).Value;
            mockGallery.Owner = 1;

            var mockApprovedItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Approved, "jpg");
            var mockPendingItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Pending, "jpg");

            var mockItems = new List<GalleryItemModel>();
            mockItems.AddRange(mockApprovedItems);
            mockItems.AddRange(mockPendingItems);

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(2);
            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(false);

            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);

            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));

            _database.GetGalleryIdentifier(Arg.Any<int>()).Returns(new GalleryIdentifierModel() { Id = mockGallery.Id, Identifier = mockGallery.Identifier, Name = mockGallery.Name });

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(mockGallery.Identifier, mockGallery.SecretKey);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(mockGallery.Id));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(mockGallery.Identifier));
            Assert.That(model?.Gallery?.Name, Is.EqualTo(mockGallery.Name));
            Assert.That(model?.Images, Is.Not.Null);
            Assert.That(model.Images.Count, Is.EqualTo(expectedItemCount));
        }

        [TestCase(GalleryType.Basic, 10)]
        [TestCase(GalleryType.Drop, 10)]
        [TestCase(GalleryType.Collection, 10)]
        public async Task GalleryController_Index_PrivilegedUsers_ShowPendingUploads_Enabled(GalleryType type, int expectedItemCount)
        {
            var mockGallery = GetMockGalleryData().FirstOrDefault(x => x.Value.Type == type).Value;
            mockGallery.Owner = 1;

            var mockApprovedItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Approved, "jpg");
            var mockPendingItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Pending, "jpg");

            var mockItems = new List<GalleryItemModel>();
            mockItems.AddRange(mockApprovedItems);
            mockItems.AddRange(mockPendingItems);

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(2);
            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(false);

            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);

            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));

            _database.GetGalleryIdentifier(Arg.Any<int>()).Returns(new GalleryIdentifierModel() { Id = mockGallery.Id, Identifier = mockGallery.Identifier, Name = mockGallery.Name });

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(mockGallery.Identifier, mockGallery.SecretKey);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(mockGallery.Id));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(mockGallery.Identifier));
            Assert.That(model?.Gallery?.Name, Is.EqualTo(mockGallery.Name));
            Assert.That(model?.Images, Is.Not.Null);
            Assert.That(model.Images.Count, Is.EqualTo(expectedItemCount));
        }

        [TestCase(GalleryType.Basic, 5)]
        [TestCase(GalleryType.Drop, 5)]
        [TestCase(GalleryType.Collection, 5)]
        public async Task GalleryController_Index_GalleryOwner_ShowPendingUploads_Disabled(GalleryType type, int expectedItemCount)
        {
            var mockGallery = GetMockGalleryData().FirstOrDefault(x => x.Value.Type == type).Value;
            mockGallery.Owner = 1;

            var mockApprovedItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Approved, "jpg");
            var mockPendingItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Pending, "jpg");

            var mockItems = new List<GalleryItemModel>();
            mockItems.AddRange(mockApprovedItems);
            mockItems.AddRange(mockPendingItems);

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(1);
            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(true);

            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(false);

            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));

            _database.GetGalleryIdentifier(Arg.Any<int>()).Returns(new GalleryIdentifierModel() { Id = mockGallery.Id, Identifier = mockGallery.Identifier, Name = mockGallery.Name });

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(mockGallery.Identifier, mockGallery.SecretKey);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(mockGallery.Id));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(mockGallery.Identifier));
            Assert.That(model?.Gallery?.Name, Is.EqualTo(mockGallery.Name));
            Assert.That(model?.Images, Is.Not.Null);
            Assert.That(model.Images.Count, Is.EqualTo(expectedItemCount));
        }

        [TestCase(GalleryType.Basic, 10)]
        [TestCase(GalleryType.Drop, 10)]
        [TestCase(GalleryType.Collection, 10)]
        public async Task GalleryController_Index_GalleryOwner_ShowPendingUploads_Enabled(GalleryType type, int expectedItemCount)
        {
            var mockGallery = GetMockGalleryData().FirstOrDefault(x => x.Value.Type == type).Value;
            mockGallery.Owner = 1;

            var mockApprovedItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Approved, "jpg");
            var mockPendingItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Pending, "jpg");

            var mockItems = new List<GalleryItemModel>();
            mockItems.AddRange(mockApprovedItems);
            mockItems.AddRange(mockPendingItems);

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(1);
            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(true);

            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);

            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));

            _database.GetGalleryIdentifier(Arg.Any<int>()).Returns(new GalleryIdentifierModel() { Id = mockGallery.Id, Identifier = mockGallery.Identifier, Name = mockGallery.Name });

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(mockGallery.Identifier, mockGallery.SecretKey);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(mockGallery.Id));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(mockGallery.Identifier));
            Assert.That(model?.Gallery?.Name, Is.EqualTo(mockGallery.Name));
            Assert.That(model?.Images, Is.Not.Null);
            Assert.That(model.Images.Count, Is.EqualTo(expectedItemCount));
        }

        [TestCase(GalleryType.Basic, 5)]
        [TestCase(GalleryType.Drop, 0)]
        [TestCase(GalleryType.Collection, 5)]
        public async Task GalleryController_Index_NonGalleryOwner_ShowPendingUploads_Disabled(GalleryType type, int expectedItemCount)
        {
            var mockGallery = GetMockGalleryData().FirstOrDefault(x => x.Value.Type == type).Value;
            mockGallery.Owner = 1;

            var mockApprovedItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Approved, "jpg");
            var mockPendingItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Pending, "jpg");

            var mockItems = new List<GalleryItemModel>();
            mockItems.AddRange(mockApprovedItems);
            mockItems.AddRange(mockPendingItems);

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(20);
            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(false);

            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(false);

            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));

            _database.GetGalleryIdentifier(Arg.Any<int>()).Returns(new GalleryIdentifierModel() { Id = mockGallery.Id, Identifier = mockGallery.Identifier, Name = mockGallery.Name });

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(mockGallery.Identifier, mockGallery.SecretKey);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(mockGallery.Id));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(mockGallery.Identifier));
            Assert.That(model?.Gallery?.Name, Is.EqualTo(mockGallery.Name));
            Assert.That(model?.Images, Is.Not.Null);
            Assert.That(model.Images.Count, Is.EqualTo(expectedItemCount));
        }

        [TestCase(GalleryType.Basic, 5)]
        [TestCase(GalleryType.Drop, 0)]
        [TestCase(GalleryType.Collection, 5)]
        public async Task GalleryController_Index_NonGalleryOwner_ShowPendingUploads_Enabled(GalleryType type, int expectedItemCount)
        {
            var mockGallery = GetMockGalleryData().FirstOrDefault(x => x.Value.Type == type).Value;
            mockGallery.Owner = 1;

            var mockApprovedItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Approved, "jpg");
            var mockPendingItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Pending, "jpg");

            var mockItems = new List<GalleryItemModel>();
            mockItems.AddRange(mockApprovedItems);
            mockItems.AddRange(mockPendingItems);

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(20);
            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(false);

            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);

            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));

            _database.GetGalleryIdentifier(Arg.Any<int>()).Returns(new GalleryIdentifierModel() { Id = mockGallery.Id, Identifier = mockGallery.Identifier, Name = mockGallery.Name });

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(mockGallery.Identifier, mockGallery.SecretKey);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(mockGallery.Id));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(mockGallery.Identifier));
            Assert.That(model?.Gallery?.Name, Is.EqualTo(mockGallery.Name));
            Assert.That(model?.Images, Is.Not.Null);
            Assert.That(model.Images.Count, Is.EqualTo(expectedItemCount));
        }

        [TestCase(GalleryType.Basic, 5)]
        [TestCase(GalleryType.Drop, 0)]
        [TestCase(GalleryType.Collection, 5)]
        public async Task GalleryController_Index_AnonymousGuest_ShowPendingUploads_Disabled(GalleryType type, int expectedItemCount)
        {
            var mockGallery = GetMockGalleryData().FirstOrDefault(x => x.Value.Type == type).Value;
            mockGallery.Owner = 1;

            var mockApprovedItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Approved, "jpg");
            var mockPendingItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Pending, "jpg");

            var mockItems = new List<GalleryItemModel>();
            mockItems.AddRange(mockApprovedItems);
            mockItems.AddRange(mockPendingItems);

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(-1);
            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(false);

            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(false);

            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));

            _database.GetGalleryIdentifier(Arg.Any<int>()).Returns(new GalleryIdentifierModel() { Id = mockGallery.Id, Identifier = mockGallery.Identifier, Name = mockGallery.Name });

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(mockGallery.Identifier, mockGallery.SecretKey);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(mockGallery.Id));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(mockGallery.Identifier));
            Assert.That(model?.Gallery?.Name, Is.EqualTo(mockGallery.Name));
            Assert.That(model?.Images, Is.Not.Null);
            Assert.That(model.Images.Count, Is.EqualTo(expectedItemCount));
        }

        [TestCase(GalleryType.Basic, 5)]
        [TestCase(GalleryType.Drop, 0)]
        [TestCase(GalleryType.Collection, 5)]
        public async Task GalleryController_Index_AnonymousGuest_ShowPendingUploads_Enabled(GalleryType type, int expectedItemCount)
        {
            var mockGallery = GetMockGalleryData().FirstOrDefault(x => x.Value.Type == type).Value;
            mockGallery.Owner = 1;

            var mockApprovedItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Approved, "jpg");
            var mockPendingItems = MockData.MockGalleryItems(5, mockGallery.Id, GalleryItemState.Pending, "jpg");

            var mockItems = new List<GalleryItemModel>();
            mockItems.AddRange(mockApprovedItems);
            mockItems.AddRange(mockPendingItems);

            _identity.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns(-1);
            _identity.IsValid(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsBasicUser(Arg.Any<ClaimsPrincipal>()).Returns(true);
            _identity.IsPrivilegedUser(Arg.Any<ClaimsPrincipal>()).Returns(false);
            _identity.IsOwner(Arg.Any<ClaimsPrincipal>(), Arg.Any<int?>()).Returns(false);

            _deviceDetector.ParseDeviceType(Arg.Any<string>()).Returns(DeviceType.Desktop);
            _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, Arg.Any<bool>()).Returns(true);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, Arg.Any<bool>(), Arg.Any<int>()).Returns(true);

            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetGalleryItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.All, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Approved, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockApprovedItems));
            _database.GetCollectionItems(Arg.Any<int?>(), mockGallery.Id, GalleryItemState.Pending, Arg.Any<MediaType>(), Arg.Any<ImageOrientation>(), Arg.Any<GalleryGroup>(), Arg.Any<GalleryOrder>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(mockPendingItems));

            _database.GetGalleryIdentifier(Arg.Any<int>()).Returns(new GalleryIdentifierModel() { Id = mockGallery.Id, Identifier = mockGallery.Identifier, Name = mockGallery.Name });

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext();

            ViewResult actual = (ViewResult)await controller.Index(mockGallery.Identifier, mockGallery.SecretKey);
            Assert.That(actual, Is.TypeOf<ViewResult>());
            Assert.That(actual?.Model, Is.Not.Null);

            PhotoGallery model = (PhotoGallery)actual.Model;
            Assert.That(model?.Gallery?.Id, Is.EqualTo(mockGallery.Id));
            Assert.That(model?.Gallery?.Identifier, Is.EqualTo(mockGallery.Identifier));
            Assert.That(model?.Gallery?.Name, Is.EqualTo(mockGallery.Name));
            Assert.That(model?.Images, Is.Not.Null);
            Assert.That(model.Images.Count, Is.EqualTo(expectedItemCount));
        }

        [TestCase(true, 1, null, false)]
        [TestCase(true, 3, "Bob", false)]
        [TestCase(false, 1, "", false)]
        [TestCase(false, 3, "Unit Testing", false)]
        [TestCase(false, 1, "Logged In", true)]
        public async Task GalleryController_UploadImage(bool requiresReview, int fileCount, string? uploadedBy, bool loggedIn)
        {
            _database.GetUser(Arg.Any<int>()).Returns(loggedIn ? new UserModel() { Firstname = "Logged", Lastname = "In" } : null);
            _settings.GetOrDefault(MemtlyConfiguration.Gallery.RequireReview, Arg.Any<bool>()).Returns(requiresReview);

            var files = new FormFileCollection();
            for (var i = 0; i < fileCount; i++)
            {
                files.Add(new FormFile(null, 0, 0, "TestFile_001", $"{Guid.NewGuid()}.jpg"));
            }

            var session = new MockSession();
            session.Set(SessionKey.Viewer.Identity, uploadedBy ?? string.Empty);

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext(
                session: session,
                form: new Dictionary<string, StringValues>
                {
                    { "CollectionId", "0" },
                    { "GalleryId", "1" },
                    { "SecretKey", "password" }
                },
                files: files);

            JsonResult actual = (JsonResult)await controller.UploadImage();
            Assert.That(actual, Is.TypeOf<JsonResult>());
            Assert.That(actual?.Value, Is.Not.Null);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.True);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(files.Count));
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploadedBy", string.Empty), Is.EqualTo(!string.IsNullOrWhiteSpace(uploadedBy) ? uploadedBy : string.Empty));
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.EqualTo(0));
        }

        [TestCase]
        public async Task GalleryController_UploadImage_Duplicate()
        {
            _database.GetUser(Arg.Any<int>()).Returns(new UserModel());
            _database.GetGalleryItemByChecksum(Arg.Any<int>(), Arg.Any<string>()).Returns(Task.FromResult(MockData.MockGalleryItems(1, 1, GalleryItemState.Approved).FirstOrDefault()));

            var files = new FormFileCollection();
            files.Add(new FormFile(null, 0, 0, "TestFile_001", $"{Guid.NewGuid()}.jpg"));

            var session = new MockSession();
            session.Set(SessionKey.Viewer.Identity, string.Empty);

            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext(
                session: session,
                form: new Dictionary<string, StringValues>
                {
                    { "CollectionId", "0" },
                    { "GalleryId", "1" },
                    { "SecretKey", "password" }
                },
                files: files);

            JsonResult actual = (JsonResult)await controller.UploadImage();
            Assert.That(actual, Is.TypeOf<JsonResult>());
            Assert.That(actual?.Value, Is.Not.Null);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task GalleryController_UploadImage_InvalidGallery(string? id)
        {
            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext(form: new Dictionary<string, StringValues>
            {
                { "CollectionId", "0" },
                { "GalleryId", "1" },
            });

            JsonResult actual = (JsonResult)await controller.UploadImage();
            Assert.That(actual, Is.TypeOf<JsonResult>());
            Assert.That(actual?.Value, Is.Not.Null);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task GalleryController_UploadImage_InvalidSecretKey(string? key)
        {
            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext(form: new Dictionary<string, StringValues>
            {
                { "CollectionId", "0" },
                { "GalleryId", "1" },
                { "SecretKey", key }
            });

            JsonResult actual = (JsonResult)await controller.UploadImage();
            Assert.That(actual, Is.TypeOf<JsonResult>());
            Assert.That(actual?.Value, Is.Not.Null);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
        }

        [TestCase()]
        public async Task GalleryController_UploadImage_MissingGallery()
        {
            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext(form: new Dictionary<string, StringValues>
            {
                { "CollectionId", Guid.NewGuid().ToString() },
                { "GalleryId", "1" }
            });

            JsonResult actual = (JsonResult)await controller.UploadImage();
            Assert.That(actual, Is.TypeOf<JsonResult>());
            Assert.That(actual?.Value, Is.Not.Null);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
        }

        [TestCase()]
        public async Task GalleryController_UploadImage_NoFiles()
        {
            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext(form: new Dictionary<string, StringValues>
            {
                { "CollectionId", "0" },
                { "GalleryId", "1" },
                { "SecretKey", "password" }
            });

            JsonResult actual = (JsonResult)await controller.UploadImage();
            Assert.That(actual, Is.TypeOf<JsonResult>());
            Assert.That(actual?.Value, Is.Not.Null);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
        }

        [TestCase()]
        public async Task GalleryController_UploadImage_FileTooBig()
        {
            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext(
                form: new Dictionary<string, StringValues>
                {
                    { "CollectionId", "0" },
                    { "GalleryId", "1" },
                    { "SecretKey", "password" }
                },
                files: new FormFileCollection() {
                    new FormFile(null, 0, int.MaxValue, "TestFile_001", $"{Guid.NewGuid()}.jpg")
                });

            JsonResult actual = (JsonResult)await controller.UploadImage();
            Assert.That(actual, Is.TypeOf<JsonResult>());
            Assert.That(actual?.Value, Is.Not.Null);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
        }

        [TestCase()]
        public async Task GalleryController_UploadImage_InvalidFileType()
        {
            var controller = new GalleryController(_settings, _database, _file, _deviceDetector, _image, _notification, _encryption, _url, _identity, _logger, _localizer);
            controller.ControllerContext.HttpContext = MockData.MockHttpContext(
                form: new Dictionary<string, StringValues>
                {
                    { "CollectionId", "0" },
                    { "GalleryId", "1" },
                    { "SecretKey", "password" }
                },
                files: new FormFileCollection() {
                    new FormFile(null, 0, int.MaxValue, "TestFile_001", $"{Guid.NewGuid()}.blaa")
                });

            JsonResult actual = (JsonResult)await controller.UploadImage();
            Assert.That(actual, Is.TypeOf<JsonResult>());
            Assert.That(actual?.Value, Is.Not.Null);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "success", false), Is.False);
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "uploaded", 0), Is.EqualTo(0));
            Assert.That(JsonResponseHelper.GetPropertyValue(actual.Value, "errors", new List<string>()).Count, Is.GreaterThan(0));
        }

        private IDictionary<string, GalleryModel> GetMockGalleryData()
        {
            return new Dictionary<string, GalleryModel>()
            {
                {
                    "default", new GalleryModel()
                    {
                        Id = 1,
                        Identifier = "default",
                        Name = "default",
                        SecretKey = "password",
                        ApprovedItems = 32,
                        PendingItems = 50,
                        TotalItems = 72,
                        Owner = 1,
                        Type = GalleryType.Basic
                    }
                },
                {
                    "blaa", new GalleryModel()
                    {
                        Id = 2,
                        Identifier = "blaa",
                        Name = "blaa",
                        SecretKey = "456789",
                        ApprovedItems = 2,
                        PendingItems = 1,
                        TotalItems = 3,
                        Owner = 1,
                        Type = GalleryType.Collection
                    }
                },
                {
                    "drop_test", new GalleryModel()
                    {
                        Id = 3,
                        Identifier = "drop_test",
                        Name = "drop_test",
                        SecretKey = "123456",
                        ApprovedItems = 0,
                        PendingItems = 0,
                        TotalItems = 0,
                        Owner = 1,
                        Type = GalleryType.Drop
                    }
                },
                {
                    "missing", new GalleryModel()
                    {
                        Id = 101,
                        Identifier = "missing",
                        Name = "missing",
                        SecretKey = "123456",
                        ApprovedItems = 0,
                        PendingItems = 0,
                        TotalItems = 0,
                        Owner = 1,
                        Type = GalleryType.Basic
                    }
                }
            };
        }
    }
}