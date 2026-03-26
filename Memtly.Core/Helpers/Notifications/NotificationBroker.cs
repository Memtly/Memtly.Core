using Memtly.Core.Constants;
using Microsoft.Extensions.Localization;

namespace Memtly.Core.Helpers.Notifications
{
    public class NotificationBroker : INotificationHelper
    {
        private readonly ISettingsHelper _settings;
        private readonly ISmtpClientWrapper _smtp;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILoggerFactory _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        public NotificationBroker(ISettingsHelper settings, ISmtpClientWrapper smtp, IHttpClientFactory clientFactory, ILoggerFactory logger, IStringLocalizer<Localization.Translations> localizer)
        {
            _settings = settings;
            _smtp = smtp;
            _clientFactory = clientFactory;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<bool> Send(string title, string message, string? actionLink = null)
        {
            var emailSent = true;
            var ntfySent = true;
            var gotifySent = true;

            if (await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Smtp.Enabled, false))
            {
                emailSent = await new EmailHelper(_settings, _smtp, _logger.CreateLogger<EmailHelper>(), _localizer).Send(title, message, actionLink);
            }

            if (await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Ntfy.Enabled, false))
            {
                ntfySent = await new NtfyHelper(_settings, _clientFactory, _logger.CreateLogger<NtfyHelper>()).Send(title, message, actionLink);
            }

            if (await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Gotify.Enabled, false))
            {
                gotifySent = await new GotifyHelper(_settings, _clientFactory, _logger.CreateLogger<GotifyHelper>()).Send(title, message, actionLink);
            }

            return emailSent && ntfySent && gotifySent;
        }
    }
}