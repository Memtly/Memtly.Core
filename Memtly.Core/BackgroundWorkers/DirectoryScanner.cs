using System.Reflection;
using Memtly.Core.Constants;
using Memtly.Core.Enums;
using Memtly.Core.Helpers;
using Memtly.Core.Helpers.Database;
using Memtly.Core.Models.Database;
using NCrontab;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Memtly.Core.BackgroundWorkers
{
    public sealed class DirectoryScanner : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISettingsHelper _settingsHelper;
        private readonly IFileHelper _fileHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IAuditHelper _auditHelper;
        private readonly ILogger<DirectoryScanner> _logger;

        public DirectoryScanner(IServiceScopeFactory scopeFactory, ISettingsHelper settingsHelper, IFileHelper fileHelper, IImageHelper imageHelper, IAuditHelper auditHelper, ILogger<DirectoryScanner> logger)
        {
            _scopeFactory = scopeFactory;
            _settingsHelper = settingsHelper;
            _fileHelper = fileHelper;
            _imageHelper = imageHelper;
            _auditHelper = auditHelper;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enabled = await _settingsHelper.GetOrDefault(MemtlyConfiguration.BackgroundServices.DirectoryScanner.Enabled, true);
            if (enabled)
            {
                var cron = await _settingsHelper.GetOrDefault(MemtlyConfiguration.BackgroundServices.DirectoryScanner.Schedule, "*/30 * * * *");
                var nextExecutionTime = DateTime.Now.AddMinutes(5);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var currentCron = await _settingsHelper.GetOrDefault(MemtlyConfiguration.BackgroundServices.DirectoryScanner.Schedule, "*/30 * * * *");

                    var now = DateTime.Now;
                    if (now >= nextExecutionTime)
                    {
                        await ScanForFiles();

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

        private async Task ScanForFiles()
        {
            await this.ScanGalleryImages();
            await this.ScanCustomResources();
        }

        private async Task ScanGalleryImages()
        {
            try
            {
                var rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
                var thumbnailsDirectory = Path.Combine(rootDirectory, Directories.Public.Thumbnails);
                _fileHelper.CreateDirectoryIfNotExists(thumbnailsDirectory);

                var uploadsDirectory = Path.Combine(rootDirectory, Directories.Public.Uploads);
                if (_fileHelper.DirectoryExists(uploadsDirectory))
                {
                    var galleryDirs = _fileHelper.GetDirectories(uploadsDirectory, "*", SearchOption.TopDirectoryOnly)?.Where(x => !Path.GetFileName(x).StartsWith("."));
                    if (galleryDirs != null)
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                            
                            var systemUser = await db.GetUserByUsername(UserAccounts.SystemUser);

                            foreach (var galleryDir in galleryDirs)
                            {
                                try
                                {
                                    var galleryName = Path.GetFileName(galleryDir).ToLower();
                                    var identifier = galleryName;

                                    var galleryId = await db.GetGalleryId(identifier);
                                    if (galleryId == null && await db.GetGalleryCount() < await _settingsHelper.GetOrDefault(MemtlyConfiguration.Basic.MaxGalleryCount, 1000000))
                                    {
                                        identifier = GalleryHelper.IsValidGalleryIdentifier(galleryName) ? galleryName : GalleryHelper.GenerateGalleryIdentifier();
                                        galleryId = (await db.AddGallery(new GalleryModel()
                                        {
                                            Identifier = identifier,
                                            Name = galleryName,
                                            SecretKey = PasswordHelper.GenerateGallerySecretKey(),
                                            Owner = systemUser!.Id
                                        }))?.Id;
                                        await _auditHelper.LogAction($"Directory scanner added new gallery '{identifier}'", AuditSeverity.Verbose);
                                    }

                                    if (galleryId != null)
                                    {
                                        var galleryItem = await db.GetGallery(galleryId.Value);
                                        if (galleryItem != null)
                                        {
                                            var galleryPath = Path.Combine(uploadsDirectory, galleryItem.Identifier);
                                            if (!galleryDir.Equals(galleryPath))
                                            {
                                                _fileHelper.MoveDirectoryIfExists(galleryDir, galleryPath);
                                            }

                                            var allowedFileTypes = _settingsHelper.GetOrDefault(MemtlyConfiguration.Gallery.AllowedFileTypes, ".jpg,.jpeg,.png,.mp4,.mov", galleryItem?.Id).Result.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                            var galleryItems = await db.GetGalleryItems(null, galleryItem!.Id);

                                            if (Path.Exists(galleryPath))
                                            {
                                                var approvedFiles = _fileHelper.GetFiles(galleryPath, "*.*", SearchOption.TopDirectoryOnly).Where(x => allowedFileTypes.Any(y => string.Equals(Path.GetExtension(x).Trim('.'), y.Trim('.'), StringComparison.OrdinalIgnoreCase)));
                                                if (approvedFiles != null)
                                                {
                                                    foreach (var file in approvedFiles)
                                                    {
                                                        try
                                                        {
                                                            var filename = Path.GetFileName(file);
                                                            var g = galleryItems.FirstOrDefault(x => string.Equals(x.Title, filename, StringComparison.OrdinalIgnoreCase));
                                                            if (g == null)
                                                            {
                                                                g = await db.AddGalleryItem(new GalleryItemModel()
                                                                {
                                                                    GalleryId = galleryItem.Id,
                                                                    Title = filename,
                                                                    Checksum = await _fileHelper.GetChecksum(file),
                                                                    MediaType = _imageHelper.GetMediaType(file),
                                                                    State = GalleryItemState.Approved,
                                                                    UploadedDate = await _fileHelper.GetCreationDatetime(file),
                                                                    FileSize = _fileHelper.FileSize(file)
                                                                });
                                                                await _auditHelper.LogAction($"Directory scanner added new approved item '{filename}' to gallery '{identifier}'", AuditSeverity.Verbose);
                                                            }

                                                            var thumbnailDir = Path.Combine(thumbnailsDirectory, galleryItem.Identifier);
                                                            var thumbnailPath = Path.Combine(thumbnailDir, $"{Path.GetFileNameWithoutExtension(file)}.webp");
                                                            if (!_fileHelper.FileExists(thumbnailPath))
                                                            {
                                                                _fileHelper.CreateDirectoryIfNotExists(thumbnailDir);
                                                                await _imageHelper.GenerateThumbnail(file, thumbnailPath, _settingsHelper.GetOrDefault(MemtlyConfiguration.Basic.ThumbnailSize, 720).Result);
                                                                _fileHelper.DeleteFileIfExists(Path.Combine(thumbnailsDirectory, $"{Path.GetFileNameWithoutExtension(file)}.webp"));
                                                            }
                                                            else
                                                            {
                                                                using (var img = await Image.LoadAsync(thumbnailPath))
                                                                {
                                                                    var width = img.Width;

                                                                    img.Mutate(x => x.AutoOrient());

                                                                    if (width != img.Width)
                                                                    {
                                                                        await img.SaveAsWebpAsync(thumbnailPath);
                                                                    }
                                                                }
                                                            }

                                                            if (g != null)
                                                            {
                                                                var updated = false;

                                                                if (g.MediaType == MediaType.Unknown)
                                                                {
                                                                    g.MediaType = _imageHelper.GetMediaType(file);
                                                                    updated = true;
                                                                }

                                                                if (g.Orientation == ImageOrientation.Unknown)
                                                                {
                                                                    g.Orientation = await _imageHelper.GetOrientation(thumbnailPath);
                                                                    updated = true;
                                                                }

                                                                if (g.FileSize == 0)
                                                                {
                                                                    g.FileSize = _fileHelper.FileSize(file);
                                                                    updated = true;
                                                                }

                                                                if (updated)
                                                                {
                                                                    await db.EditGalleryItem(g);
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            _logger.LogError(ex, $"An error occurred while scanning file '{file}'");
                                                        }
                                                    }
                                                }

                                                if (Path.Exists(Path.Combine(galleryPath, "Pending")))
                                                {
                                                    var pendingFiles = _fileHelper.GetFiles(Path.Combine(galleryPath, "Pending"), "*.*", SearchOption.TopDirectoryOnly).Where(x => allowedFileTypes.Any(y => string.Equals(Path.GetExtension(x).Trim('.'), y.Trim('.'), StringComparison.OrdinalIgnoreCase)));
                                                    if (pendingFiles != null)
                                                    {
                                                        foreach (var file in pendingFiles)
                                                        {
                                                            try
                                                            {
                                                                var filename = Path.GetFileName(file);
                                                                if (!galleryItems.Exists(x => string.Equals(x.Title, filename, StringComparison.OrdinalIgnoreCase)))
                                                                {
                                                                    await db.AddGalleryItem(new GalleryItemModel()
                                                                    {
                                                                        GalleryId = galleryItem.Id,
                                                                        Title = filename,
                                                                        Checksum = await _fileHelper.GetChecksum(file),
                                                                        MediaType = _imageHelper.GetMediaType(file),
                                                                        State = GalleryItemState.Pending,
                                                                        UploadedDate = await _fileHelper.GetCreationDatetime(file),
                                                                        FileSize = new FileInfo(file).Length
                                                                    });
                                                                    await _auditHelper.LogAction($"Directory scanner added new pending item '{filename}' to gallery '{identifier}'", AuditSeverity.Verbose);
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                _logger.LogError(ex, $"An error occurred while scanning file '{file}'");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"An error occurred while scanning directory '{galleryDir}'");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DirectoryScanner - ScanGalleryImages - Failed to scan files - {ex?.Message}");
            }
        }

        private async Task ScanCustomResources()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                    var systemUser = await db.GetUserByUsername(UserAccounts.SystemUser);

                    var existing = await db.GetCustomResources();

                    var customResourcesDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, Directories.Public.CustomResources);
                    _fileHelper.CreateDirectoryIfNotExists(customResourcesDirectory);

                    foreach (var resource in _fileHelper.GetFiles(customResourcesDirectory))
                    {
                        try
                        {
                            var filename = Path.GetFileName(resource);
                            if (!existing.Any(x => filename.Equals(x.FileName, StringComparison.OrdinalIgnoreCase)))
                            {
                                await db.AddCustomResource(new CustomResourceModel()
                                {
                                    Title = Path.GetFileNameWithoutExtension(filename),
                                    FileName = filename,
                                    Owner = systemUser!.Id,
                                    OwnerName = "DirectoryScanner"
                                });
                                await _auditHelper.LogAction($"Directory scanner added new custom resource '{filename}'", AuditSeverity.Verbose);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DirectoryScanner - ScanCustomResources - Failed to scan files - {ex?.Message}");
            }
        }
    }
}