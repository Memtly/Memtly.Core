using Memtly.Core.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class HtmlSanitizerTests
    {
        public HtmlSanitizerTests()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("<div><b>Test</b></div>", new[] { "b" }, "<div>Test</div>")]
        [TestCase("<div><b>Test</b></div>", new[] { "div" }, "<b>Test</b>")]
        [TestCase("<div><b>Test</b></div>", new[] { "div", "b" }, "Test")]
        [TestCase("<script src=\"asdasd\">Test</script>", new[] { "script" }, "Test")]
        [TestCase("<script src=\"asdasd\"/>", new[] { "script" }, "")]
        [TestCase("<img src=\"asdasd\"/>", new[] { "img" }, "")]
        [TestCase("Blaa<div><b>Test</b></div>", new[] { ".*" }, "Blaa")]
        public void HtmlSanitizer_SanitizeHtmlTags(string input, string[] tags, string expected)
        {
            var actual = HtmlSanitizer.SanitizeHtmlTags(input, tags);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("<script src=\"blaa\" link=\"unit\"></script>", new[] { "src" }, "<script  link=\"unit\"></script>")]
        [TestCase("<script src=\"blaa\" link=\"unit\"></script>", new[] { "link" }, "<script src=\"blaa\" ></script>")]
        [TestCase("<script src=\"blaa\" link=\"unit\"></script>", new[] { ".*" }, "></script>")]
        public void HtmlSanitizer_SanitizeHtmlAttributes(string input, string[] tags, string expected)
        {
            var actual = HtmlSanitizer.SanitizeHtmlAttributes(input, tags);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("this is a https://unit.com/ test", "this is a  test")]
        [TestCase("this is a https://www.unit.com/ test", "this is a  test")]
        [TestCase("this is a https://www.unit.com/home test", "this is a  test")]
        [TestCase("this is a http://unit.com/ test", "this is a  test")]
        [TestCase("this is a http://www.unit.com/ test", "this is a  test")]
        [TestCase("this is a http://www.unit.com/home test", "this is a  test")]
        public void HtmlSanitizer_SanitizeHtmlAttributes(string input, string expected)
        {
            var actual = HtmlSanitizer.SanitizeLinks(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("this is a test", false)]
        [TestCase("this is a <img src=x onerror=alert(0)> test", true)]
        [TestCase("this is a <script/> test", true)]
        [TestCase("this is a <script></script> test", true)]
        [TestCase("this is a http://www.unit.com/home test", true)]
        [TestCase("this is a https://www.unit.com/home test", true)]
        public void HtmlSanitizer_MayContainXss(string input, bool expected)
        {
            var actual = HtmlSanitizer.MayContainXss(input);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}