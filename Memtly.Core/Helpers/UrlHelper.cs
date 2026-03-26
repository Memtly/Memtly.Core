using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Memtly.Core.Constants;

namespace Memtly.Core.Helpers
{
    public interface IUrlHelper
    {
        public string GenerateFullUrl(HttpRequest? ctx, string? path, List<KeyValuePair<string, string>>? append = null, List<string>? exclude = null);
        public string GenerateBaseUrl(HttpRequest? ctx, string? path);
        public string GenerateQueryString(HttpRequest? ctx, List<KeyValuePair<string, string>>? append = null, List<string>? exclude = null);
        public string ExtractHost(string value);
        public string ExtractQueryValue(HttpRequest? ctx, string key);
    }

    public class UrlHelper : IUrlHelper
    {
        private readonly ISettingsHelper _settings;

        public UrlHelper(ISettingsHelper settings)
        {
            _settings = settings;
        }

        public string GenerateFullUrl(HttpRequest? ctx, string? path, List<KeyValuePair<string, string>>? append = null, List<string>? exclude = null)
        {
            return $"{GenerateBaseUrl(ctx, path)}{GenerateQueryString(ctx, append, exclude)}";
        }

        public string GenerateBaseUrl(HttpRequest? ctx, string? path)
        {
            if (ctx != null)
            {
                var scheme = _settings.GetOrDefault(MemtlyConfiguration.Basic.ForceHttps, false).Result ? "https" : ctx.Scheme;
                var host = ExtractHost(_settings.GetOrDefault(MemtlyConfiguration.Basic.BaseUrl, ctx.Host.Value).Result);

                return $"{scheme}://{host}/{path?.TrimStart('/')}";
            }

            return string.Empty;
        }

        public string GenerateQueryString(HttpRequest? ctx, List<KeyValuePair<string, string>>? append = null, List<string>? exclude = null)
        {
            if (ctx != null)
            {
                append = append ?? new List<KeyValuePair<string, string>>();
                exclude = exclude ?? new List<string>();
                
                foreach (var a in append)
                {
                    exclude.Add(a.Key);
                }

                var queryString = new StringBuilder();
                foreach (var q in ctx.Query.Where(x => !exclude.Any(f => f.Equals(x.Key, StringComparison.OrdinalIgnoreCase))))
                {
                    queryString.Append($"&{HttpUtility.UrlEncode(q.Key)}={HttpUtility.UrlEncode(q.Value)}");
                }

                foreach (var a in append)
                {
                    queryString.Append($"&{HttpUtility.UrlEncode(a.Key)}={HttpUtility.UrlEncode(a.Value)}");
                }

                return $"?{queryString.ToString().Trim('&')}".TrimEnd('?');
            }

            return string.Empty;
        }

        public string ExtractHost(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return Regex.Replace(value, "http[s]*\\:\\/\\/", string.Empty).Trim('/');
            }

            return string.Empty;
        }

        public string ExtractQueryValue(HttpRequest? ctx, string key)
        {
            if (ctx?.Query != null)
            { 
                try
                {
                    return ctx.Query.FirstOrDefault(x => string.Equals(key, x.Key, StringComparison.OrdinalIgnoreCase)).Value.ToString();
                }
                catch { }
            }

            return string.Empty;
        }
    }
}