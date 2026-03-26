using Microsoft.AspNetCore.Http;
using Memtly.Core.Constants;
using Memtly.Core.Helpers;
using Memtly.Core.UnitTests.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class UrlHelperTests
    {
        private readonly ISettingsHelper _settings = Substitute.For<ISettingsHelper>();

        public UrlHelperTests()
        {
        }

        [SetUp]
        public void Setup()
        {
            _settings.GetOrDefault(MemtlyConfiguration.Basic.ForceHttps, Arg.Any<bool>()).Returns(false);
        }

        [TestCase("http", "unittest.com", null, "http://unittest.com/")]
        [TestCase("https", "unittest.org", null, "https://unittest.org/")]
        [TestCase("http", "www.unittest.com", null, "http://www.unittest.com/")]
        [TestCase("https", "mobile.unittest.org", null, "https://mobile.unittest.org/")]
        [TestCase("http", "unittest.com", "", "http://unittest.com/")]
        [TestCase("https", "unittest.org", "", "https://unittest.org/")]
        [TestCase("http", "www.unittest.com", "", "http://www.unittest.com/")]
        [TestCase("https", "mobile.unittest.org", "", "https://mobile.unittest.org/")]
        [TestCase("http", "unittest.com", "/unittest", "http://unittest.com/unittest")]
        [TestCase("https", "unittest.org", "?unit=test", "https://unittest.org/?unit=test")]
        [TestCase("http", "www.unittest.com", "?unit=test&blaa=test", "http://www.unittest.com/?unit=test&blaa=test")]
        [TestCase("https", "mobile.unittest.org", "/unittest?unit=test&blaa=test", "https://mobile.unittest.org/unittest?unit=test&blaa=test")]
        public void UrlHelper_GenerateBaseUrl(string scheme, string host, string? querystring, string expected)
        {
            _settings.GetOrDefault(MemtlyConfiguration.Basic.BaseUrl, Arg.Any<string>()).Returns(host);

            var mockContext = MockData.MockHttpContext();
            mockContext.Request.Scheme = scheme;
            mockContext.Request.Host = new HostString(host);

            var actual = new UrlHelper(_settings).GenerateBaseUrl(mockContext?.Request, querystring);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("?a=123&b=456&c=789", "d=111", "", "?a=123&b=456&c=789&d=111")]
        [TestCase("?a=123&b=456&c=789", "d=111,e=222", "", "?a=123&b=456&c=789&d=111&e=222")]
        [TestCase("?a=123&b=456&c=789", "e=222,f=333,d=111", "", "?a=123&b=456&c=789&e=222&f=333&d=111")]
        [TestCase("?a=123&b=456&c=789", "d=t$&", "", "?a=123&b=456&c=789&d=t%24%26")]
        public void UrlHelper_GenerateQueryString_Append(string queryString, string append, string exclude, string expected)
        {
            var mockContext = MockData.MockHttpContext();
            mockContext.Request.Scheme = "https";
            mockContext.Request.Host = new HostString("unit.test.com");
            mockContext.Request.QueryString = new QueryString(queryString);

            var include = append?.Split(',', StringSplitOptions.RemoveEmptyEntries)?.Select(x => {
                var val = x.Split('=');
                return new KeyValuePair<string, string>(val.FirstOrDefault() ?? "default", val.LastOrDefault() ?? "default");
            })?.ToList();

            var actual = new UrlHelper(_settings).GenerateQueryString(mockContext?.Request, include, exclude?.Split(',', StringSplitOptions.RemoveEmptyEntries)?.ToList());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("?a=123&b=456&c=789", "", "", "?a=123&b=456&c=789")]
        [TestCase("?a=123&b=456&c=789", "", "d", "?a=123&b=456&c=789")]
        [TestCase("?a=123&b=456&c=789", "", "a", "?b=456&c=789")]
        [TestCase("?a=123&b=456&c=789", "", "b", "?a=123&c=789")]
        [TestCase("?a=123&b=456&c=789", "", "c", "?a=123&b=456")]
        public void UrlHelper_GenerateQueryString_Exclude(string queryString, string append, string exclude, string expected)
        {
            var mockContext = MockData.MockHttpContext();
            mockContext.Request.Scheme = "https";
            mockContext.Request.Host = new HostString("unit.test.com");
            mockContext.Request.QueryString = new QueryString(queryString);

            var include = append?.Split(',', StringSplitOptions.RemoveEmptyEntries)?.Select(x => {
                var val = x.Split('=');
                return new KeyValuePair<string, string>(val.FirstOrDefault() ?? "default", val.LastOrDefault() ?? "default");
            })?.ToList();

            var actual = new UrlHelper(_settings).GenerateQueryString(mockContext?.Request, include, exclude?.Split(',', StringSplitOptions.RemoveEmptyEntries)?.ToList());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("?a=123&b=456&c=789", "d=111", "a", "?b=456&c=789&d=111")]
        [TestCase("?a=123&b=456&c=789", "d=111,e=222", "b", "?a=123&c=789&d=111&e=222")]
        [TestCase("?a=123&b=456&c=789", "e=222,f=333,d=111", "c", "?a=123&b=456&e=222&f=333&d=111")]
        [TestCase("?a=123&b=456&c=789", "d=111,e=222,f=333", "e", "?a=123&b=456&c=789&d=111&e=222&f=333")]
        public void UrlHelper_GenerateQueryString_AppendExclude(string queryString, string append, string exclude, string expected)
        {
            var mockContext = MockData.MockHttpContext();
            mockContext.Request.Scheme = "https";
            mockContext.Request.Host = new HostString("unit.test.com");
            mockContext.Request.QueryString = new QueryString(queryString);

            var include = append?.Split(',', StringSplitOptions.RemoveEmptyEntries)?.Select(x => {
                var val = x.Split('=');
                return new KeyValuePair<string, string>(val.FirstOrDefault() ?? "default", val.LastOrDefault() ?? "default");
            })?.ToList();

            var actual = new UrlHelper(_settings).GenerateQueryString(mockContext?.Request, include, exclude?.Split(',', StringSplitOptions.RemoveEmptyEntries)?.ToList());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("unit.test.com", "unit.test.com")]
        [TestCase("unit.test.com/", "unit.test.com")]
        [TestCase("http://unit.test.com", "unit.test.com")]
        [TestCase("http://unit.test.com/", "unit.test.com")]
        [TestCase("https://unit.test.com", "unit.test.com")]
        [TestCase("https://unit.test.com/", "unit.test.com")]
        [TestCase("http://test.com/", "test.com")]
        [TestCase("https://test.com/", "test.com")]
        public void UrlHelper_ExtractHost(string host, string expected)
        {
            var actual = new UrlHelper(_settings).ExtractHost(host);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("?a=123&b=456&c=789", "a", "123")]
        [TestCase("?a=123&b=456&c=789", "b", "456")]
        [TestCase("?a=123&b=456&c=789", "c", "789")]
        [TestCase("?a=123&b=456&c=789", "d", "")]
        public void UrlHelper_ExtractHost(string queryString, string key, string expected)
        {
            var mockContext = MockData.MockHttpContext();
            mockContext.Request.Scheme = "https";
            mockContext.Request.Host = new HostString("unit.test.com");
            mockContext.Request.QueryString = new QueryString(queryString);

            var actual = new UrlHelper(_settings).ExtractQueryValue(mockContext?.Request, key);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}