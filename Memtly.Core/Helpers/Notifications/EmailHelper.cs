using System.Net;
using Microsoft.Extensions.Localization;
using Razor.Templating.Core;
using Memtly.Core.Resources.Templates.Email;
using Memtly.Core.Constants;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Memtly.Core.Helpers.Notifications
{
    public class EmailHelper : INotificationHelper
    {
        private readonly ISettingsHelper _settings;
        private readonly ISmtpClientWrapper _client;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        public EmailHelper(ISettingsHelper settings, ISmtpClientWrapper client, ILogger<EmailHelper> logger, IStringLocalizer<Localization.Translations> localizer)
        {
            _settings = settings;
            _client = client;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<bool> Send(string title, string message, string? actionLink = null)
        {
            return await this.SendTo(await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Smtp.Recipient, string.Empty), title, new BasicEmail()
            {
                Title = title,
                Message = message,
                Link = !string.IsNullOrWhiteSpace(actionLink) ? new BasicEmailLink()
                {
                    Heading = _localizer["Visit"].Value,
                    Value = actionLink
                } : null
            });
        }

        public async Task<bool> SendTo(string recipients, string title, BasicEmail model)
        {
            var body = await RazorTemplateEngine.RenderAsync("~/Resources/Templates/Email/Basic.cshtml", model);
            return await this.SendTo(recipients, title, body);
        }

        public async Task<bool> SendTo(string recipients, string title, string message)
        {
            if (await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Smtp.Enabled, false))
            {
                try
                { 
                    var host = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Smtp.Host, string.Empty);
                    var port = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Smtp.Port, 587);
                    var from = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Smtp.From, string.Empty);
                    var displayName = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Smtp.DisplayName, "Memtly");
                    
                    var username = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Smtp.Username, string.Empty);
                    var password = await _settings.GetOrDefault(MemtlyConfiguration.Notifications.Smtp.Password, string.Empty);

                    NetworkCredential? credentials = null;
                    if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                    {
                        credentials = new NetworkCredential(username, password);
                    }

                    return await SendTo(host, port, from, displayName, credentials, recipients, title, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email with title '{title}' - {ex?.Message}");
                }
            }

            return false;
        }

        public async Task<bool> SendTo(string host, int port, string from, string displayName, NetworkCredential? credentials, string recipients, string title, string message)
        {
            var addressList = recipients?.Split(new char[] { ';', ',' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)?.Select(x => new MailboxAddress("Recipient", x));
            
            if (addressList != null && addressList.Any())
            { 
                if (!string.IsNullOrWhiteSpace(host))
                {
                    if (port > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(from))
                        {
                            var sentToAll = true;
                            using (var smtp = new SmtpClient())
                            {
                                var sender = new MailboxAddress(displayName, from);
                                
                                var msg = new MimeMessage();
                                msg.From.Add(sender);
                                msg.Subject = title;
                                msg.Body = new TextPart("html") { Text = message };

                                await _client.ConnectAsync(smtp, host, port, SecureSocketOptions.Auto);
                                if (!string.IsNullOrWhiteSpace(credentials?.UserName) && !string.IsNullOrWhiteSpace(credentials?.Password))
                                {
                                    await _client.AuthenticateAsync(smtp, credentials);
                                }

                                foreach (var to in addressList)
                                {
                                    try
                                    {
                                        msg.To.Clear();
                                        msg.To.Add(to);

                                        await _client.SendAsync(smtp, msg);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, $"Failed to send email to '{to}' - {ex.Message}");
                                        sentToAll = false;
                                    }
                                }

                                await _client.DisconnectAsync(smtp, true);
                            }
                
                            return sentToAll;
                        }
                        else
                        { 
                            _logger.LogWarning($"Invalid SMTP sender specified");
                        }
                    }
                    else
                    { 
                        _logger.LogWarning($"Invalid SMTP port specified");
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid SMTP host specified");
                }
            }
            else
            {
                _logger.LogWarning($"Invalid SMTP recipient specified");
            }

            return false;
        }
    }
}