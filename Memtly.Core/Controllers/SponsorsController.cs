using Memtly.Core.Constants;
using Memtly.Core.Helpers;
using Memtly.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Memtly.Core.Controllers
{
    [AllowAnonymous]
    public class SponsorsController : BaseController
    {
        private readonly ISettingsHelper _settings;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IIdentityHelper _identity;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        public SponsorsController(ISettingsHelper settings, IHttpClientFactory clientFactory, IIdentityHelper identity, ILogger<HomeController> logger, IStringLocalizer<Localization.Translations> localizer)
            : base()
        {
            _settings = settings;
            _clientFactory = clientFactory;
            _identity = identity;
            _logger = logger;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new Views.Sponsors.IndexModel();

            try
            {
                var client = _clientFactory.CreateClient("SponsorsClient");
                var endpoint = await _settings.GetOrDefault(MemtlyConfiguration.Sponsors.Endpoint, "/sponsors.json");
                model.SponsorsList = await client.GetFromJsonAsync<SponsorsList>(endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_localizer["Sponsors_Load_Error"].Value} - {ex?.Message}");
            }

            return PartialView(model);
        }
    }
}