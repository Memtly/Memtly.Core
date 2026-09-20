using System.Net;
using System.Reflection;
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
        private readonly IIdentityHelper _identity;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        private readonly string RootDirectory;
        private readonly string UploadsDirectory;
        private readonly string ThumbnailsDirectory;
        private readonly string CustomResourcesDirectory;

        public MediaViewerController(ISettingsHelper settings, IDatabaseHelper database, IIdentityHelper identity, ILogger<MediaViewerController> logger, IStringLocalizer<Localization.Translations> localizer)
            : base()
        {
            _settings = settings;
            _database = database;
            _identity = identity;
            _logger = logger;
            _localizer = localizer;

            RootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            UploadsDirectory = Path.Combine(RootDirectory, Directories.Public.Uploads);
            ThumbnailsDirectory = Path.Combine(RootDirectory, Directories.Public.Thumbnails);
            CustomResourcesDirectory = Path.Combine(RootDirectory, Directories.Public.CustomResources);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GalleryItem(int id, string? secretKey)
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
                            secretKey = secretKey ?? string.Empty;

                            if (!string.IsNullOrWhiteSpace(gallery.SecretKey) && !secretKey.Equals(gallery.SecretKey))
                            {
                                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                return Json(new { success = false, message = _localizer["Gallery_Invalid_Secret_Key"].Value });
                            }

                            var user = _identity.IsValid(User) ? User.Identity : null;
                            var identityEnabled = await _settings.GetOrDefault(MemtlyConfiguration.IdentityCheck.Enabled, true);
                            var likesEnabled = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Likes, true, galleryItem.GalleryId);
                            var commentsEnabled = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Comments, true, galleryItem.GalleryId);

                            var authorName = string.Empty;
                            var authorEmailAddress = string.Empty;

                            if (identityEnabled)
                            {
                                authorName = !string.IsNullOrWhiteSpace(galleryItem?.UploadedBy) ? galleryItem.UploadedBy : "Anonymous";

                                if (!string.IsNullOrWhiteSpace(galleryItem?.UploaderEmailAddress) && _identity.IsPrivilegedUser(User))
                                {
                                    authorEmailAddress = galleryItem.UploaderEmailAddress?.ToLower();
                                }
                            }

                            return PartialView("~/Views/MediaViewer/Popup.cshtml", new Popup()
                            {
                                Id = id,
                                Collection = gallery.Name,
                                Source = $"/{Path.Combine(UploadsDirectory, gallery.Identifier).Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{(galleryItem!.State == GalleryItemState.Pending ? "Pending/" : string.Empty)}{Uri.EscapeDataString(galleryItem.Title)}",
                                Thumbnail = $"/{Path.Combine(ThumbnailsDirectory, gallery.Identifier).Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{Uri.EscapeDataString(Path.GetFileNameWithoutExtension(galleryItem.Title))}.webp",
                                AuthorName = authorName,
                                AuthorEmailAddress = authorEmailAddress,
                                Type = galleryItem.MediaType.ToString().ToLower(),
                                State = galleryItem.State,
                                Likes = new PhotoGalleryImageLikes()
                                {
                                    Enabled = likesEnabled,
                                    CanUserLike = likesEnabled && user != null,
                                    HasUserLiked = user != null ? await _database.CheckUserHasLikedGalleryItem(galleryItem.Id, _identity.GetUserId(User)) : false,
                                    Count = await _database.GetGalleryItemLikesCount(id)
                                },
                                Comments = new PhotoGalleryImageComments()
                                {
                                    Enabled = commentsEnabled,
                                    CanUserComment = commentsEnabled && user != null,
                                    HasUserCommented = user != null ? await _database.CheckUserHasCommentedGalleryItem(galleryItem.Id, _identity.GetUserId(User)) : false,
                                    Comments = await _database.GetGalleryItemComments(id)
                                },
                                DownloadEnabled = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Download, true, gallery.Id) || _identity.IsPrivilegedUser(User)
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
                        var user = _identity.IsValid(User) ? User.Identity : null;

                        return PartialView("~/Views/MediaViewer/Popup.cshtml", new Popup()
                        {
                            Id = id,
                            Collection = "custom_resources",
                            Source = $"/{CustomResourcesDirectory.Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{Uri.EscapeDataString(resource.FileName)}",
                            Title = resource.Title,
                            AuthorName = !string.IsNullOrWhiteSpace(resource?.OwnerName) ? resource.OwnerName : "Anonymous",
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
        public async Task<IActionResult> ReviewItem(int id, string? secretKey)
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
                            secretKey = secretKey ?? string.Empty;

                            if (!string.IsNullOrWhiteSpace(gallery.SecretKey) && !secretKey.Equals(gallery.SecretKey))
                            {
                                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                return Json(new { success = false, message = _localizer["Gallery_Invalid_Secret_Key"].Value });
                            }

                            var user = _identity.IsValid(User) ? User.Identity : null;
                            var identityEnabled = await _settings.GetOrDefault(MemtlyConfiguration.IdentityCheck.Enabled, true);
                            var likesEnabled = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Likes, true, galleryItem.GalleryId);

                            var authorName = string.Empty;
                            var authorEmailAddress = string.Empty;

                            if (identityEnabled)
                            {
                                authorName = !string.IsNullOrWhiteSpace(galleryItem?.UploadedBy) ? galleryItem.UploadedBy : "Anonymous";

                                if (!string.IsNullOrWhiteSpace(galleryItem?.UploaderEmailAddress) && _identity.IsPrivilegedUser(User))
                                {
                                    authorEmailAddress = galleryItem.UploaderEmailAddress?.ToLower();
                                }
                            }

                            return PartialView("~/Views/MediaViewer/Popup.cshtml", new Popup()
                            {
                                Id = id,
                                Collection = gallery.Name,
                                Source = $"/{Path.Combine(UploadsDirectory, gallery.Identifier, "Pending").Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{Uri.EscapeDataString(galleryItem!.Title)}",
                                Thumbnail = $"/{Path.Combine(ThumbnailsDirectory, gallery.Identifier).Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{Uri.EscapeDataString(Path.GetFileNameWithoutExtension(galleryItem.Title))}.webp",
                                Title = null,
                                Description = null,
                                AuthorName = authorName,
                                AuthorEmailAddress = authorEmailAddress,
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

        [Authorize]
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
                        var likesEnabled = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Likes, true, galleryItem.GalleryId);
                        if (likesEnabled)
                        {
                            var userId = _identity.IsValid(User) ? _identity.GetUserId(User) : 0;

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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while performing action '{action}' on item '{id}' - {ex?.Message}");
                }
            }

            return Json(new { success = false });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Comment(int id, string value)
        {
            if (id > 0 && _identity.IsValid(User))
            {
                try
                {
                    var data = HtmlSanitizer.Sanitize(value);
                    if (string.IsNullOrWhiteSpace(data) || data.Length <= 0)
                    {
                        return Json(new { success = false, message = _localizer["Comment_Empty_Value"].Value });
                    }
                    else if (data.Length > 2000)
                    {
                        return Json(new { success = false, message = _localizer["Comment_Value_Too_Long"].Value });
                    }
                    else
                    {
                        var userId = _identity.GetUserId(User);
                        if (_identity.CanEdit(User, GalleryPermissions.Comment, userId))
                        {
                            var galleryItem = await _database.GetGalleryItem(id);
                            if (galleryItem != null)
                            {
                                var commentsEnabled = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Comments, true, galleryItem.GalleryId);
                                if (commentsEnabled)
                                {
                                    var comment = await _database.AddGalleryItemComment(new GalleryItemCommentModel()
                                    {
                                        GalleryId = galleryItem.GalleryId,
                                        GalleryItemId = galleryItem.Id,
                                        UserId = userId,
                                        Value = data
                                    });

                                    if (comment != null)
                                    {
                                        return Json(new { success = true, id = comment.Id, username = comment.Username, value = comment.Value, timestamp = comment.TimestampString, timestamp_full = comment.TimestampStringFull });
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while adding comment to item '{id}' - {ex?.Message}");
                }
            }

            return Json(new { success = false, message = _localizer["Unexpected_Error_Occurred"].Value });
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteComment(int id)
        {
            if (id > 0 && _identity.IsValid(User))
            {
                try
                {
                    var userId = _identity.GetUserId(User);
                    if (_identity.CanEdit(User, GalleryPermissions.Comment, userId))
                    {
                        var comment = await _database.GetGalleryItemComment(id);
                        if (comment != null)
                        {
                            var gallery = await _database.GetGallery(comment.GalleryId);
                            if (gallery != null && (_identity.IsOwner(User, gallery.Owner) || _identity.IsOwner(User, comment.UserId)))
                            {
                                await _database.DeleteGalleryItemComment(comment);

                                return Json(new { success = true });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while removing comment '{id}' - {ex?.Message}");
                }
            }

            return Json(new { success = false, message = _localizer["Unexpected_Error_Occurred"].Value });
        }
    }
}