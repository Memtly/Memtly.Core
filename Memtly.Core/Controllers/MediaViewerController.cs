using System.Reflection;
using System.Text;
using Memtly.Core.Attributes;
using Memtly.Core.Constants;
using Memtly.Core.Enums;
using Memtly.Core.Extensions;
using Memtly.Core.Helpers;
using Memtly.Core.Helpers.Database;
using Memtly.Core.Models;
using Memtly.Core.Models.Database;
using Memtly.Core.Views.MediaViewer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Memtly.Core.Controllers
{
    public class MediaViewerController : BaseController
    {
        private readonly ISettingsHelper _settings;
        private readonly IDatabaseHelper _database;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        private readonly string RootDirectory;
        private readonly string UploadsDirectory;
        private readonly string ThumbnailsDirectory;
        private readonly string CustomResourcesDirectory;

        public MediaViewerController(ISettingsHelper settings, IDatabaseHelper database, ILogger<MediaViewerController> logger, IStringLocalizer<Localization.Translations> localizer)
            : base()
        {
            _settings = settings;
            _database = database;
            _logger = logger;
            _localizer = localizer;

            RootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            UploadsDirectory = Path.Combine(RootDirectory, Directories.Public.Uploads);
            ThumbnailsDirectory = Path.Combine(RootDirectory, Directories.Public.Thumbnails);
            CustomResourcesDirectory = Path.Combine(RootDirectory, Directories.Public.CustomResources);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GalleryItem(int id)
        {
            if (id > 0)
            {
                try
                {
                    var galleryItem = await _database.GetGalleryItem(id);
                    if (galleryItem != null)
                    {
                        var gallery = await _database.GetGallery(galleryItem.GalleryId);
                        if (gallery != null)
                        { 
                            var user = User?.Identity != null && User.Identity.IsAuthenticated ? User.Identity : null;
                            var identityEnabled = await _settings.GetOrDefault(MemtlyConfiguration.IdentityCheck.Enabled, true);
                            var likesEnabled = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Likes, true, galleryItem.GalleryId);

                            var author = string.Empty;
                            if (identityEnabled)
                            {
                                var builder = new StringBuilder($"{_localizer["Uploaded_By"].Value}: ");

                                if (!string.IsNullOrWhiteSpace(galleryItem?.UploadedBy))
                                {
                                    builder.Append(galleryItem.UploadedBy);

                                    if (!string.IsNullOrWhiteSpace(galleryItem?.UploaderEmailAddress) && (user?.IsPrivilegedUser() ?? false))
                                    {
                                        builder.Append($" - {galleryItem?.UploaderEmailAddress?.ToLower()}");
                                    }
                                }
                                else
                                {
                                    builder.Append("Anonymous");
                                }

                                author = builder.ToString();
                            }

                            return PartialView("~/Views/MediaViewer/Popup.cshtml", new Popup() 
                            {
                                Id = id,
                                Collection = gallery.Name,
                                Source = $"/{Path.Combine(UploadsDirectory, gallery.Identifier).Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{galleryItem.Title}",
                                Thumbnail = $"/{Path.Combine(ThumbnailsDirectory, gallery.Identifier).Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{Path.GetFileNameWithoutExtension(galleryItem.Title)}.webp",
                                Author = author,
                                Type = galleryItem.MediaType.ToString().ToLower(),
                                Likes = new PhotoGalleryImageLikes()
                                {
                                    Enabled = likesEnabled,
                                    CanUserLike = likesEnabled && user != null,
                                    HasUserLiked = user != null ? await _database.CheckUserHasLikedGalleryItem(galleryItem.Id, user.GetUserId()) : false,
                                    Count = await _database.GetGalleryItemLikesCount(id)
                                },
                                DownloadEnabled = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Download, true, gallery.Id) || (user?.IsPrivilegedUser() ?? false)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while getting the details for item '{id}' - {ex?.Message}");
                }
            }

            return PartialView("~/Views/MediaViewer/Popup.cshtml", new Popup() { Id = id });
        }

        [Authorize]
        [HttpGet]
        [RequiresRole(CustomResourcePermission = CustomResourcePermissions.View)]
        public async Task<IActionResult> CustomResource(int id)
        {
            if (id > 0)
            {
                try
                {
                    var resource = await _database.GetCustomResource(id);
                    if (resource != null)
                    {
                        var user = User?.Identity != null && User.Identity.IsAuthenticated ? User.Identity : null;

                        return PartialView("~/Views/MediaViewer/Popup.cshtml", new Popup()
                        {
                            Id = id,
                            Collection = "custom_resources",
                            Source = $"/{CustomResourcesDirectory.Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{resource.FileName}",
                            Title = resource.Title,
                            Author = $"{_localizer["Uploaded_By"].Value}: {(!string.IsNullOrWhiteSpace(resource?.OwnerName) ? resource.OwnerName : "Anonymous")}",
                            Type = MediaType.Image.ToString().ToLower(),
                            DownloadEnabled = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while getting the details for item '{id}' - {ex?.Message}");
                }
            }

            return PartialView("~/Views/MediaViewer/Popup.cshtml", new Popup() { Id = id });
        }

        [Authorize]
        [HttpGet]
        [RequiresRole(ReviewPermission = ReviewPermissions.View)]
        public async Task<IActionResult> ReviewItem(int id)
        {
            if (id > 0)
            {
                try
                {
                    var galleryItem = await _database.GetGalleryItem(id);
                    if (galleryItem != null)
                    {
                        var gallery = await _database.GetGallery(galleryItem.GalleryId);
                        if (gallery != null)
                        {
                            var user = User?.Identity != null && User.Identity.IsAuthenticated ? User.Identity : null;
                            var identityEnabled = await _settings.GetOrDefault(MemtlyConfiguration.IdentityCheck.Enabled, true);
                            var likesEnabled = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Likes, true, galleryItem.GalleryId);

                            var author = string.Empty;
                            if (identityEnabled)
                            {
                                var builder = new StringBuilder($"{_localizer["Uploaded_By"].Value}: ");

                                if (!string.IsNullOrWhiteSpace(galleryItem?.UploadedBy))
                                {
                                    builder.Append(galleryItem.UploadedBy);

                                    if (!string.IsNullOrWhiteSpace(galleryItem?.UploaderEmailAddress) && (user?.IsPrivilegedUser() ?? false))
                                    {
                                        builder.Append($" - {galleryItem?.UploaderEmailAddress?.ToLower()}");
                                    }
                                }
                                else
                                {
                                    builder.Append("Anonymous");
                                }

                                author = builder.ToString();
                            }

                            return PartialView("~/Views/MediaViewer/Popup.cshtml", new Popup()
                            {
                                Id = id,
                                Collection = gallery.Name,
                                Source = $"/{Path.Combine(UploadsDirectory, gallery.Identifier, "Pending").Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{galleryItem.Title}",
                                Thumbnail = $"/{Path.Combine(ThumbnailsDirectory, gallery.Identifier).Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{Path.GetFileNameWithoutExtension(galleryItem.Title)}.webp",
                                Title = null,
                                Description = null,
                                Author = author,
                                Type = galleryItem.MediaType.ToString().ToLower(),
                                DownloadEnabled = false
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while getting the details for item '{id}' - {ex?.Message}");
                }
            }

            return PartialView("~/Views/MediaViewer/Popup.cshtml", new Popup() { Id = id });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Like(int id, string action)
        {
            if (id > 0)
            {
                try
                {
                    var galleryItem = await _database.GetGalleryItem(id);
                    if (galleryItem != null)
                    {
                        var userId = User?.Identity != null && User.Identity.IsAuthenticated ? User.Identity.GetUserId() : 0;

                        long likes = 0;
                        switch (action.ToLower())
                        {
                            case "like":
                                likes = await _database.LikeGalleryItem(new GalleryItemLikeModel()
                                {
                                    GalleryId = galleryItem.GalleryId,
                                    GalleryItemId = galleryItem.Id,
                                    UserId = userId
                                });
                                break;
                            case "unlike":
                                likes = await _database.UnLikeGalleryItem(new GalleryItemLikeModel()
                                {
                                    GalleryId = galleryItem.GalleryId,
                                    GalleryItemId = galleryItem.Id,
                                    UserId = userId
                                });
                                break;
                        }

                        return Json(new { success = true, value = likes });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while performing action '{action}' on item '{id}' - {ex?.Message}");
                }
            }

            return Json(new { success = false });
        }
    }
}