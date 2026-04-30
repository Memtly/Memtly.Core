using System.Text;
using Microsoft.Extensions.Localization;
using NCrontab;
using Memtly.Core.Constants;
using Memtly.Core.Helpers;
using Memtly.Core.Helpers.Database;
using Memtly.Core.Helpers.Notifications;

namespace Memtly.Core.BackgroundWorkers
{
    public sealed class NotificationReport : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISettingsHelper _settingsHelper;
        private readonly ISmtpClientWrapper _smtpHelper;
        private readonly IStringLocalizer<Localization.Translations> _localizer;
        private readonly ILogger<NotificationReport> _notificationLogger;
        private readonly ILogger<EmailHelper> _emailLogger;

        public NotificationReport(IServiceScopeFactory scopeFactory, ISettingsHelper settingsHelper, ISmtpClientWrapper smtpHelper, ILoggerFactory loggerFactory, IStringLocalizer<Localization.Translations> localizer)
        {
            _scopeFactory = scopeFactory;
            _settingsHelper = settingsHelper;
            _smtpHelper = smtpHelper;
            _localizer = localizer;
            _notificationLogger = loggerFactory.CreateLogger<NotificationReport>();
            _emailLogger = loggerFactory.CreateLogger<EmailHelper>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enabled = await _settingsHelper.GetOrDefault(MemtlyConfiguration.Reports.Email.Enabled, true);
            if (enabled)
            {
                var cron = await _settingsHelper.GetOrDefault(MemtlyConfiguration.Reports.Email.Schedule, "0 0 * * *");
                var nextExecutionTime = DateTime.Now.AddMinutes(1);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var currentCron = await _settingsHelper.GetOrDefault(MemtlyConfiguration.Reports.Email.Schedule, "0 0 * * *");

                    var now = DateTime.Now;
                    if (now >= nextExecutionTime)
                    {
                        if (await _settingsHelper.GetOrDefault(MemtlyConfiguration.Reports.Email.Enabled, true) && await _settingsHelper.GetOrDefault(MemtlyConfiguration.Notifications.Smtp.Enabled, false))
                        {
                            await SendReport();
                        }

                        var schedule = CrontabSchedule.Parse(cron, new CrontabSchedule.ParseOptions() { IncludingSeconds = cron.Split(new[] { ' ' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length == 6 });
                        nextExecutionTime = schedule.GetNextOccurrence(now);
                    }
                    else
                    {
                        if (!currentCron.Equals(cron))
                        {
                            nextExecutionTime = DateTime.Now;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }

                    cron = currentCron;
                }
            }
        }

        private async Task SendReport()
        {
            try
            {
                await Task.Run(async () =>
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                        var pendingItems = await db.GetGalleryItems();
                        if (pendingItems != null && pendingItems.Any())
                        {
                            var builder = new StringBuilder();
                            builder.AppendLine($"<h1>You have items pending review!</h1>");

                            foreach (var item in pendingItems.GroupBy(x => x.GalleryId).OrderByDescending(x => x.Count()))
                            {
                                var gallery = await db.GetGallery(item.Key);
                                if (gallery != null)
                                {
                                    try
                                    {
                                        builder.AppendLine($"<p style=\"font-size: 16pt;\">{gallery.Name} - Pending Items ({item.Count()})</p>");
                                    }
                                    catch (Exception ex)
                                    {
                                        _notificationLogger.LogError(ex, $"Failed to build gallery report for '{gallery.Name}' - {ex?.Message}");
                                    }
                                }
                            }

                            var sent = await new EmailHelper(_settingsHelper, _smtpHelper, _emailLogger, _localizer).Send("Pending Items Report", builder.ToString());
                            if (!sent)
                            {
                                _notificationLogger.LogWarning($"Failed to send notification report");
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _notificationLogger.LogError(ex, $"NotificationReport - Failed to send report - {ex?.Message}");
            }
        }
    }
}