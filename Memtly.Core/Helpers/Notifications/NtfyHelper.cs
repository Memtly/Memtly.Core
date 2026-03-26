using System.Net.Http.Headers;
using Memtly.Core.Constants;

namespace Memtly.Core.Helpers.Notifications
{
    public class NtfyHelper : INotificationHelper
    {
        private readonly ISettingsHelper _settings;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;

        public NtfyHelper(ISettingsHelper settings, IHttpClientFactory clientFactory, ILogger<NtfyHelper> logger)
        {
            _settings = settings;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<bool> Send(string title, string message, string? actionLink = null)
        {
            if (await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Ntfy.Enabled, false))
            {
                try
                {
                    var endpoint = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Ntfy.Endpoint, string.Empty);
                    var token = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Ntfy.Token, string.Empty);
                    var topic = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Ntfy.Topic, "Memtly");
                    var priority = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Ntfy.Priority, 4);

                    return await Send(endpoint, topic, token, priority, title, message, actionLink);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send Ntfy message with title '{title}' - {ex?.Message}");
                }
            }

            return false;
        }

        public async Task<bool> Send(string endpoint, string topic, string token, int priority, string title, string message, string? actionLink = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                _logger.LogWarning($"Invalid Ntfy endpoint specified");
                return false;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning($"No Ntfy token specified. It is recommended that you secure your topic with a token");
            }
 
            if (!string.IsNullOrWhiteSpace(topic))
            {
                if (priority > 0)
                {
                    var defaultIcon = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Ntfy.Icon, string.Empty);
                    var icon = await _settings.GetOrDefault(MemtlyConfiguration.Basic.Logo, defaultIcon);
                    icon = !icon.StartsWith('.') && !icon.StartsWith('/') ? icon : defaultIcon;

                    var client = _clientFactory.CreateClient("NtfyClient");

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    client.BaseAddress = new Uri(endpoint);
                        
                    using (var response = await client.PostAsJsonAsync("/", new { icon, topic, title, message, priority, click = actionLink }))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            _logger.LogError($"Failed to send Ntfy message with title '{title}' - {error}");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid Ntfy priority specified");
                }
            }
            else
            {
                _logger.LogWarning($"Invalid Ntfy topic specified");
            }

            return false;
        }
    }
}