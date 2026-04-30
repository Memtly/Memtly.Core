using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Memtly.Core.Constants;
using Memtly.Core.Helpers;

namespace Memtly.Core.Configurations
{
    public static class LocalizationConfiguration
    {
        public static string CurrentCulture = "en-GB";

        public static void AddLocalizationConfiguration(this IServiceCollection services)
        {
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            var bsp = services.BuildServiceProvider();
            var settings = bsp.GetRequiredService<ISettingsHelper>();
            services.Configure<RequestLocalizationOptions>(options => {
                var supportedCultures = new LanguageHelper().DetectSupportedCultures();

                var language = settings.GetOrDefault(MemtlyConfiguration.Languages.Default, "en-GB").Result;
                CurrentCulture = GetDefaultCulture(supportedCultures, language);
                
                options.DefaultRequestCulture = new RequestCulture(CurrentCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
        }

        private static string GetDefaultCulture(List<CultureInfo> supported, string key)
        {
            try
            {
                foreach (var culture in supported)
                {
                    if (CultureMatches(culture, key))
                    {
                        return culture.Name;
                    }
                }
            }
            catch
            {
            }

            return "en-GB";
        }

        private static bool CultureMatches(CultureInfo culture, string key)
        {
            return string.Equals(culture.Name, key, StringComparison.OrdinalIgnoreCase) || string.Equals(culture.ThreeLetterISOLanguageName, key, StringComparison.OrdinalIgnoreCase) || string.Equals(culture.TwoLetterISOLanguageName, key, StringComparison.OrdinalIgnoreCase);
        }
    }
}