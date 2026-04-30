using System.Reflection;
using Memtly.Core.Constants;
using Memtly.Core.Helpers;
using Memtly.Core.Helpers.Database;
using NCrontab;

namespace Memtly.Core.BackgroundWorkers
{
    public sealed class CleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISettingsHelper _settingsHelper;
        private readonly IFileHelper _fileHelper;
        private readonly ILogger<CleanupService> _logger;

        public CleanupService(IServiceScopeFactory scopeFactory, ISettingsHelper settingsHelper, IFileHelper fileHelper, ILogger<CleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _settingsHelper = settingsHelper;
            _fileHelper = fileHelper;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enabled = await _settingsHelper.GetOrDefault(MemtlyConfiguration.BackgroundServices.Cleanup.Enabled, true);
            if (enabled)
            {
                var cron = await _settingsHelper.GetOrDefault(MemtlyConfiguration.BackgroundServices.Cleanup.Schedule, "0 4 * * *");
                var nextExecutionTime = DateTime.Now.AddMinutes(1);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var currentCron = await _settingsHelper.GetOrDefault(MemtlyConfiguration.BackgroundServices.Cleanup.Schedule, "0 4 * * *");

                    var now = DateTime.Now;
                    if (now >= nextExecutionTime)
                    {
                        await LinkStaleGalleryItemLikes();
                        await Cleanup();
                        await FlushOldAuditLogs();

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

        private async Task Cleanup()
        {
            try
            {
                await Task.Run(() =>
                {
                    var paths = new List<string>()
                    {
                        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, Directories.Public.TempFiles)
                    };

                    if (paths != null)
                    {
                        foreach (var path in paths)
                        {
                            try
                            {
                                _fileHelper.DeleteDirectoryIfExists(path);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"An error occurred while running cleanup of '{path}'");
                            }
                        }
                    }
                });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"CleanupService - Failed to clean up files - {ex?.Message}");
            }
        }

        private async Task LinkStaleGalleryItemLikes()
        {
            try
            {
                await Task.Run(async () =>
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                        var staleLikes = await db.GetUnassignedGalleryItemLikes();
                        if (staleLikes != null && staleLikes.Any())
                        {
                            foreach (var staleLike in staleLikes.Where(x => x?.GalleryItemId != null && x.GalleryItemId > 0))
                            {
                                await db.UnLikeGalleryItem(staleLike);

                                var galleryItem = await db.GetGalleryItem(staleLike.GalleryItemId);
                                if (galleryItem?.GalleryId != null && galleryItem?.GalleryId > 0)
                                {
                                    staleLike.GalleryId = galleryItem.GalleryId;
                                    await db.LikeGalleryItem(staleLike);
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CleanupService - Failed to link stale gallery item likes - {ex?.Message}");
            }
        }

        private async Task FlushOldAuditLogs()
        {
            try
            {
                var days = await _settingsHelper.GetOrDefault(MemtlyConfiguration.Audit.Retention, 30);
                if (days > 0)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                        await db.FlushLogsOlderThan(days);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CleanupService - Failed to flush old audit logs - {ex?.Message}");
            }
        }
    }
}