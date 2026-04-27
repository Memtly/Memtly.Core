using System.Globalization;
using System.Reflection;

namespace Memtly.Core.Helpers
{
    public interface ILanguageHelper
    {
        public List<CultureInfo> DetectSupportedCultures();
        public Task<List<CultureInfo>> DetectSupportedCulturesAsync();
        public Task<bool> IsCultureSupported(string culture);
        public Task<string> GetOrFallbackCulture(string culture, string fallback);
    }

    public class LanguageHelper : ILanguageHelper
    {
        public List<CultureInfo> DetectSupportedCultures()
        {
            var supportedCultures = new List<CultureInfo>();

            try
            {
                var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) ?? string.Empty;
                var resourceFiles = Directory.GetFiles(basePath, "Memtly.Localization.resources.dll", SearchOption.AllDirectories);
                var detectedCultures = resourceFiles
                    .Select(x => Path.GetFileNameWithoutExtension(Path.GetDirectoryName(x)))
                    .Where(x => x.Contains("-"))
                    .Select(x => x.Split('.').LastOrDefault());

                foreach (var detectedCulture in detectedCultures)
                {
                    if (!string.IsNullOrWhiteSpace(detectedCulture))
                    {
                        try
                        {
                            supportedCultures.Add(new CultureInfo(detectedCulture));
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                supportedCultures.Add(new CultureInfo("en-GB"));
            }

            return supportedCultures;
        }

        public Task<List<CultureInfo>> DetectSupportedCulturesAsync()
        {
            return Task.Run(DetectSupportedCultures);
        }

        public async Task<bool> IsCultureSupported(string culture)
        {
            return this.IsCultureSupported(culture, await DetectSupportedCulturesAsync());
        }

        public bool IsCultureSupported(string culture, List<CultureInfo> supported)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(culture) && supported.Any(x => string.Equals(x.Name, culture, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        public async Task<string> GetOrFallbackCulture(string culture, string fallback)
        {
            return this.GetOrFallbackCulture(culture, fallback, await DetectSupportedCulturesAsync());
        }

        public string GetOrFallbackCulture(string culture, string fallback, List<CultureInfo> supported)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(culture))
                {
                    var match = supported.FirstOrDefault(x => string.Equals(x.Name, culture, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        return match.Name;
                    }
                }

                if (!string.IsNullOrWhiteSpace(fallback))
                {
                    var match = supported.FirstOrDefault(x => string.Equals(x.Name, fallback, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        return match.Name;
                    }
                }
            }
            catch { }

            return "en-GB";
        }
    }
}