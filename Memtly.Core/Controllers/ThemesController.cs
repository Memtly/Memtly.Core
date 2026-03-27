using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Memtly.Core.Constants;
using Memtly.Core.Enums;
using Memtly.Core.Helpers;
using Memtly.Core.Models;
using System.Reflection;

namespace Memtly.Core.Controllers
{
    [AllowAnonymous]
    public class ThemesController : BaseController
    {
        private readonly ISettingsHelper _settings;
        private readonly ILogger<ThemesController> _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        public ThemesController(ISettingsHelper settings, ILanguageHelper languageHelper, ILogger<ThemesController> logger, IStringLocalizer<Localization.Translations> localizer)
            : base()
        {
            _settings = settings;
            _logger = logger;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var options = new SupportedThemeList();

            try
            {
                var selectedTheme = HttpContext.Session.GetString(SessionKey.Theme.Selected);
                if (string.IsNullOrWhiteSpace(selectedTheme))
                {
                    selectedTheme = await _settings.GetOrDefault(MemtlyConfiguration.Themes.Default, Themes.AutoDetect.ToString());
                }

                foreach (Themes item in Enum.GetValues(typeof(Themes)))
                {
                    if (MemtlyCore.Version == MemtlyVersion.Community && (item == Themes.Green || item == Themes.DarkGreen))
                    {
                        continue;
                    }

                    options.Themes.Add(new SupportedThemes()
                        {
                            Name = _localizer[item.ToString()].Value,
                            Value = item.ToString(),
                            Selected = selectedTheme.Equals(item.ToString(), StringComparison.OrdinalIgnoreCase)
                        });
                }
            }
            catch { }

            return Json(options);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeDisplayTheme(string theme)
        {
            try
            {
                var selectedTheme = await _settings.GetOrDefault(MemtlyConfiguration.Themes.Default, Themes.AutoDetect.ToString());
                foreach (Themes item in Enum.GetValues(typeof(Themes)))
                {
                    if (MemtlyCore.Version == MemtlyVersion.Community && (item == Themes.Green || item == Themes.DarkGreen))
                    {
                        continue;
                    }

                    if (item.ToString().Equals(theme, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedTheme = item.ToString();
                        break;
                    }
                }

                HttpContext.Session.SetString(SessionKey.Theme.Selected, selectedTheme);
                Response.Cookies.Append(
                    SessionKey.Theme.Selected,
                    selectedTheme,
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true }
                );

                return Json(new { success = true });
            }
            catch (Exception ex) 
            {
                _logger.LogWarning(ex, $"Failed to set display theme to '{theme}' - {ex?.Message}");

                theme = Enums.Themes.AutoDetect.ToString();

                HttpContext.Session.SetString(SessionKey.Theme.Selected, theme);
                Response.Cookies.Append(
                    SessionKey.Theme.Selected,
                    theme,
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true }
                );
            }

            return Json(new { success = false });
        }
    }
}