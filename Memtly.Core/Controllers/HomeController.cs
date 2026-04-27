using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Memtly.Core.Constants;
using Memtly.Core.Enums;
using Memtly.Core.Extensions;
using Memtly.Core.Helpers;
using Memtly.Core.Helpers.Database;

namespace Memtly.Core.Controllers
{
    [AllowAnonymous]
    public class HomeController : BaseController
    {
        private readonly ISettingsHelper _settings;
        private readonly IDatabaseHelper _database;
        private readonly IDeviceDetector _deviceDetector;
        private readonly IAuditHelper _audit;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        public HomeController(ISettingsHelper settings, IDatabaseHelper database, IDeviceDetector deviceDetector, IAuditHelper audit, ILogger<HomeController> logger, IStringLocalizer<Localization.Translations> localizer)
            : base()
        {
            _settings = settings;
            _database = database;
            _deviceDetector = deviceDetector;
            _audit = audit;
            _logger = logger;
            _localizer = localizer;
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index()
        {
            var model = new Views.Home.IndexModel();

            try
            {
                var deviceType = HttpContext.Session.GetString(SessionKey.Device.Type);
                if (string.IsNullOrWhiteSpace(deviceType))
                {
                    deviceType = (await _deviceDetector.ParseDeviceType(Request.Headers["User-Agent"].ToString())).ToString();
                    HttpContext.Session.SetString(SessionKey.Device.Type, deviceType ?? "Desktop");
                }

                if (await _settings.GetOrDefault(MemtlyConfiguration.Basic.SingleGalleryMode, false))
                {
                    var gallery = await _database.GetGallery(1);
                    if (string.IsNullOrWhiteSpace(gallery?.SecretKey))
                    {
                        return RedirectToAction("Index", "Gallery", new { identifier = "default" });
                    }
                }

                var isDropdownMode = await _settings.GetOrDefault(MemtlyConfiguration.GallerySelector.Dropdown, false);
                var showGalleryNames = await _settings.GetOrDefault(MemtlyConfiguration.GallerySelector.ShowGalleryNames, true);
                var showGalleryIdentifiers = await _settings.GetOrDefault(MemtlyConfiguration.GallerySelector.ShowGalleryIdentifiers, true);
                var showUsernames = await _settings.GetOrDefault(MemtlyConfiguration.GallerySelector.ShowUsernames, true);

                var galleryNames = isDropdownMode ? (await _database.GetGalleryNames(showGalleryNames, showGalleryIdentifiers, showUsernames)).Where(x => !x.Value.Equals(SystemGalleries.AllGallery, StringComparison.OrdinalIgnoreCase)) : new Dictionary<string, string>();
                if (await _settings.GetOrDefault(MemtlyConfiguration.GallerySelector.HideDefaultOption, false))
                {
                    galleryNames = galleryNames.Where(x => 
                        !x.Key.Equals(SystemGalleries.DefaultGallery, StringComparison.OrdinalIgnoreCase)
                        && !x.Key.Equals($"{SystemGalleries.DefaultGallery} - {SystemGalleries.DefaultGallery}", StringComparison.OrdinalIgnoreCase)
                        && !x.Key.Equals($"{SystemGalleries.DefaultGallery} - {UserAccounts.SystemUser}", StringComparison.OrdinalIgnoreCase)
                        && !x.Key.Equals($"{SystemGalleries.DefaultGallery} - {UserAccounts.AdminUser}", StringComparison.OrdinalIgnoreCase)
                        && !x.Key.Equals($"{SystemGalleries.DefaultGallery} - {SystemGalleries.DefaultGallery} - {UserAccounts.SystemUser}", StringComparison.OrdinalIgnoreCase)
                        && !x.Key.Equals($"{SystemGalleries.DefaultGallery} - {SystemGalleries.DefaultGallery} - {UserAccounts.AdminUser}", StringComparison.OrdinalIgnoreCase)
                    );
                }

                model.GalleryNames = galleryNames.OrderBy(gallery => gallery.Key.Equals("default", StringComparison.OrdinalIgnoreCase) ? 0 : 1).ThenBy(gallery => gallery.Value.ToLower()).ToDictionary();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_localizer["Homepage_Load_Error"].Value} - {ex?.Message}");
            }

            return View(model);
        }

        [HttpGet]
        [Route("CookiePolicy")]
        [Route("Home/CookiePolicy")]
        public async Task<IActionResult> CookiePolicy()
        {
            ViewBag.CompanyName = await _settings.GetOrDefault(MemtlyConfiguration.Basic.Title, "Memtly");
            ViewBag.SiteHostname = await _settings.GetOrDefault(MemtlyConfiguration.Basic.BaseUrl, "www.memtly.com");
            ViewBag.CustomPolicy = await _settings.GetOrDefault(MemtlyConfiguration.Policies.CookiePolicy, string.Empty);

            return View("~/Views/Home/CookiePolicy.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> SetIdentity(string name, string? emailAddress)
        {
            try
            {
                var emailRequired = await _settings.GetOrDefault(MemtlyConfiguration.IdentityCheck.RequireEmail, false);

                if (string.IsNullOrWhiteSpace(name) || HtmlSanitizer.MayContainXss(name))
                {
                    return Json(new { success = false, reason = 1 });
                }
                else if (emailRequired && (string.IsNullOrWhiteSpace(emailAddress) || EmailValidationHelper.IsValid(emailAddress) == false || HtmlSanitizer.MayContainXss(emailAddress)))
                {
                    return Json(new { success = false, reason = 2 });
                }
                else
                {
                    HttpContext.Session.SetString(SessionKey.Viewer.Identity, name);
                    HttpContext.Session.SetString(SessionKey.Viewer.EmailAddress, emailAddress ?? string.Empty);

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_localizer["Identity_Session_Error"].Value}: '{name}'");
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> LogCookieApproval()
        {
            try
            {
                var ipAddress = Request.HttpContext.TryGetIpAddress();

                return Json(new { success = await _audit.LogAction($"{_localizer["Audit_CookieConsentApproved"].Value}: {ipAddress}", AuditSeverity.Verbose) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_localizer["Cookie_Audit_Error"].Value}");
            }

            return Json(new { success = false });
        }
    }
}