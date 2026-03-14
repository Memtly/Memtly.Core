using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Memtly.Core.Enums;
using Memtly.Core.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class ImageHelperTests
    {
        private readonly IFileHelper _fileHelper = Substitute.For<IFileHelper>();
        private readonly ILogger<ImageHelper> _logger = Substitute.For<ILogger<ImageHelper>>();
        private readonly IStringLocalizer<Memtly.Localization.Translations> _localizer = Substitute.For<IStringLocalizer<Memtly.Localization.Translations>>();
        private readonly IDictionary<ImageOrientation, Image?> _imageCollection;

        public ImageHelperTests()
        {
            _imageCollection = new Dictionary<ImageOrientation, Image?>()
            {
                { ImageOrientation.Square, new Image<Rgba32>(100, 100) },
                { ImageOrientation.Landscape, new Image<Rgba32>(200, 100) },
                { ImageOrientation.Portrait, new Image<Rgba32>(100, 200) }
            };
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase(ImageOrientation.Square)]
        [TestCase(ImageOrientation.Landscape)]
        [TestCase(ImageOrientation.Portrait)]
        public void ImageHelper_GetOrientation(ImageOrientation orientation)
        {
            var image = _imageCollection[orientation];
            Assert.IsNotNull(image);

            var actual = new ImageHelper(_fileHelper, _logger, _localizer).GetOrientation(image);
            Assert.That(actual, Is.EqualTo(orientation));
        }
    }
}