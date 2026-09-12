using System.Net;
using System.Reflection;
using System.Text;
using Memtly.Core.Attributes;
using Memtly.Core.Constants;
using Memtly.Core.Enums;
using Memtly.Core.Extensions;
using Memtly.Core.Helpers;
using Memtly.Core.Helpers.Database;
using Memtly.Core.Helpers.Notifications;
using Memtly.Core.Models;
using Memtly.Core.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Memtly.Core.Controllers
{
    [AllowAnonymous]
    public class GalleryController : BaseController
    {
        private readonly ISettingsHelper _settings;
        private readonly IDatabaseHelper _database;
        private readonly IFileHelper _fileHelper;
        private readonly IDeviceDetector _deviceDetector;
        private readonly IImageHelper _imageHelper;
        private readonly INotificationHelper _notificationHelper;
        private readonly IEncryptionHelper _encryptionHelper;
        private readonly Helpers.IUrlHelper _urlHelper;
        private readonly IIdentityHelper _identity;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        private readonly string RootDirectory;
        private readonly string AssetsDirectory;
        private readonly string TempDirectory;
        private readonly string UploadsDirectory;
        private readonly string ThumbnailsDirectory;

        public GalleryController(ISettingsHelper settings, IDatabaseHelper database, IFileHelper fileHelper, IDeviceDetector deviceDetector, IImageHelper imageHelper, INotificationHelper notificationHelper, IEncryptionHelper encryptionHelper, Helpers.IUrlHelper urlHelper, IIdentityHelper identity, ILogger<GalleryController> logger, IStringLocalizer<Localization.Translations> localizer)
            : base()
        {
            _settings = settings;
            _database = database;
            _fileHelper = fileHelper;
            _deviceDetector = deviceDetector;
            _imageHelper = imageHelper;
            _notificationHelper = notificationHelper;
            _encryptionHelper = encryptionHelper;
            _urlHelper = urlHelper;
            _identity = identity;
            _logger = logger;
            _localizer = localizer;

            RootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            AssetsDirectory = Path.Combine(RootDirectory, Directories.Private.Assets);
            TempDirectory = Path.Combine(RootDirectory, Directories.Public.TempFiles);
            UploadsDirectory = Path.Combine(RootDirectory, Directories.Public.Uploads);
            ThumbnailsDirectory = Path.Combine(RootDirectory, Directories.Public.Thumbnails);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string identifier)
        {
            int? galleryId = 0;

            if (!string.IsNullOrWhiteSpace(identifier))
            {
                galleryId = await _database.GetGalleryId(identifier.ToLower());
            }

            GalleryModel? gallery = galleryId != null ? await _database.GetGallery(galleryId.Value) : null;
            if (string.IsNullOrWhiteSpace(gallery?.Identifier))
            {
                return await ErrorResponse(ErrorCode.InvalidGalleryId);
            }

            return View(new Views.Gallery.LoginModel() 
            {
                Identifier = gallery.Identifier
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(string? identifier, string? key = null)
        {
            int? galleryId = 0;

            if (!string.IsNullOrWhiteSpace(identifier))
            {
                galleryId = await _database.GetGalleryId(identifier.ToLower());
            }

            GalleryModel? gallery = galleryId != null ? await _database.GetGallery(galleryId.Value) : null;
            if (gallery == null)
            {
                if (_identity.IsPrivilegedUser(User) || await _settings.GetOrDefault(MemtlyConfiguration.Basic.GuestGalleryCreation, false))
                { 
                    if (await _database.GetGalleryCount() < await _settings.GetOrDefault(MemtlyConfiguration.Basic.MaxGalleryCount, 1000000))
                    {
                        var galleryOwner = _identity.GetUserId(User);
                        if (galleryOwner <= 0)
                        {
                            var systemAccount = await _database.GetUserByUsername(UserAccounts.SystemUser);
                            if (systemAccount != null)
                            {
                                galleryOwner = systemAccount.Id;
                            }
                        }

                        if (galleryOwner > 0)
                        {
                            identifier = GalleryHelper.IsValidGalleryIdentifier(identifier?.ToLower()) ? identifier!.ToLower() : GalleryHelper.GenerateGalleryIdentifier();
                            gallery = await _database.AddGallery(new GalleryModel()
                            {
                                Identifier = identifier,
                                Name = identifier,
                                SecretKey = key,
                                Owner = galleryOwner,
                                Type = GalleryType.Basic
                            });
                        }
                        else
                        {
                            return await ErrorResponse(ErrorCode.GalleryCreationNotAllowed);
                        }
                    }
                    else
                    {
                        return await ErrorResponse(ErrorCode.GalleryLimitReached);
                    }
                }
                else
                {
                    return await ErrorResponse(ErrorCode.GalleryCreationNotAllowed);
                }
            }

            if (string.IsNullOrWhiteSpace(gallery?.Identifier))
            {
                return await ErrorResponse(ErrorCode.InvalidGalleryId);
            }

            var append = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("identifier", gallery!.Identifier)
            };

            if (!string.IsNullOrWhiteSpace(key))
            {
                var enc = _encryptionHelper.IsEncryptionEnabled();
                append.Add(new KeyValuePair<string, string>("key", enc ? _encryptionHelper.Encrypt(key) : key));
                append.Add(new KeyValuePair<string, string>("enc", enc.ToString().ToLower()));
            }

            var redirectUrl = _urlHelper.GenerateFullUrl(HttpContext.Request, "/Gallery", append);

            return new JsonResult(new { success = true, redirectUrl });
        }

        [HttpGet]
        [RequiresSecretKey]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(string? identifier, string? key = null, ViewMode? mode = null, GalleryGroup? group = null, GalleryOrder? order = null, GalleryFilter? filter = null, string? culture = null, bool partial = false, bool pagination = false, string term = "")
        {
            int? galleryId = null;

            if (!string.IsNullOrWhiteSpace(identifier))
            {
                galleryId = await _database.GetGalleryId(identifier.ToLower());
            }

            if (galleryId != null)
            {
                var userPermissions = _identity.GetUserPermissions(User);
                var userId = _identity.GetUserId(User);

                if (galleryId < 1 && !userPermissions.Gallery.HasFlag(GalleryPermissions.ViewAllGallery))
                {
                    return await ErrorResponse(ErrorCode.InvalidGalleryId);
                }

                if (!string.IsNullOrWhiteSpace(culture))
                {
                    try
                    {
                        HttpContext.Session.SetString(SessionKey.Language.Selected, culture);
                        Response.Cookies.Append(
                            CookieRequestCultureProvider.DefaultCookieName,
                            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true }
                        );
                    }
                    catch { }
                }

                try
                {
                    ViewBag.ViewMode = mode ?? (ViewMode)await _settings.GetOrDefault(MemtlyConfiguration.Gallery.DefaultView, (int)ViewMode.Default, galleryId);
                }
                catch
                {
                    ViewBag.ViewMode = ViewMode.Default;
                }

                var deviceType = HttpContext.Session.GetString(SessionKey.Device.Type);
                if (string.IsNullOrWhiteSpace(deviceType))
                {
                    deviceType = (await _deviceDetector.ParseDeviceType(Request.Headers["User-Agent"].ToString())).ToString();
                    HttpContext.Session.SetString(SessionKey.Device.Type, deviceType ?? "Desktop");
                }

                GalleryModel? gallery = await _database.GetGallery(galleryId.Value);
                if (gallery != null)
                {
                    var galleryPath = Path.Combine(UploadsDirectory, gallery.Identifier);
                    _fileHelper.CreateDirectoryIfNotExists(galleryPath);
                    _fileHelper.CreateDirectoryIfNotExists(Path.Combine(galleryPath, "Pending"));

                    ViewBag.GalleryIdentifier = gallery.Identifier;
                    ViewBag.SecretKey = gallery.SecretKey;

                    var currentPage = 1;
                    try
                    {
                        currentPage = int.Parse(_urlHelper!.ExtractQueryValue(Request, "page", "1")!.ToLower());
                    }
                    catch { }

                    var galleryGroup = group ?? (GalleryGroup)(await _settings.GetOrDefault(MemtlyConfiguration.Gallery.DefaultGroup, (int)GalleryGroup.None, gallery?.Id));
                    var galleryOrder = order ?? (GalleryOrder)(await _settings.GetOrDefault(MemtlyConfiguration.Gallery.DefaultOrder, (int)GalleryOrder.Descending, gallery?.Id));
                    var galleryFilter = filter ?? (GalleryFilter)(await _settings.GetOrDefault(MemtlyConfiguration.Gallery.DefaultFilter, (int)GalleryFilter.All, gallery?.Id));

                    var mediaType = MediaType.All;
                    if (mode == ViewMode.Slideshow)
                    {
                        mediaType = MediaType.Image;
                    }
                    else
                    {
                        switch (galleryFilter)
                        {
                            case GalleryFilter.Images:
                                mediaType = MediaType.Image;
                                break;
                            case GalleryFilter.Videos:
                                mediaType = MediaType.Video;
                                break;
                            default:
                                mediaType = MediaType.All;
                                break;
                        }
                    }

                    var orientation = ImageOrientation.All;
                    switch (galleryFilter)
                    {
                        case GalleryFilter.Landscape:
                            orientation = ImageOrientation.Landscape;
                            break;
                        case GalleryFilter.Portrait:
                            orientation = ImageOrientation.Portrait;
                            break;
                        case GalleryFilter.Square:
                            orientation = ImageOrientation.Square;
                            break;
                        default:
                            orientation = ImageOrientation.All;
                            break;
                    }

                    var itemsPerPage = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.ItemsPerPage, 50, gallery?.Id);
                    var allowedFileTypes = (await _settings.GetOrDefault(MemtlyConfiguration.Gallery.AllowedFileTypes, ".jpg,.jpeg,.png,.mp4,.mov", gallery?.Id)).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    var showPendingUploads = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, true, gallery?.Id);

                    List<GalleryItemModel>? galleryItems = null;
                    var state = showPendingUploads ? (_identity.IsValid(User) ? GalleryItemState.All : GalleryItemState.Approved) : GalleryItemState.Approved;

                    if (gallery!.Type == GalleryType.Collection && !gallery!.Identifier.Equals(SystemGalleries.AllGallery, StringComparison.OrdinalIgnoreCase))
                    {
                        galleryItems = await _database.GetCollectionItems(term, null, gallery?.Id, state, mediaType, orientation, galleryGroup, galleryOrder, currentPage, itemsPerPage);
                    }
                    else
                    {
                        galleryItems = await _database.GetGalleryItems(term, null, gallery?.Id, state, mediaType, orientation, galleryGroup, galleryOrder, currentPage, itemsPerPage);
                    }

                    var items = galleryItems?.Where(x => allowedFileTypes.Any(y => string.Equals(Path.GetExtension(x.Title).Trim('.'), y.Trim('.'), StringComparison.OrdinalIgnoreCase)));
                    if (_identity.IsBasicUser(User) && !_identity.IsOwner(User, gallery!.Owner))
                    {
                        if (gallery.Type == GalleryType.Drop)
                        {
                            items = items?.Where(x => x.UserId != null && x.UserId == userId);
                        }
                        else
                        {
                            items = items?.Where(x => x.State == GalleryItemState.Approved || (x.State == GalleryItemState.Pending && x.UserId != null && x.UserId == userId));
                        }
                    }
                    
                    var uploadActvated = !gallery!.Identifier.Equals(SystemGalleries.AllGallery, StringComparison.OrdinalIgnoreCase) && (_identity.IsOwner(User, gallery.Owner) || _identity.IsPrivilegedUser(User) || await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Upload, true, gallery?.Id));
                    if (uploadActvated)
                    {
                        try
                        {
                            var periods = (await _settings.GetOrDefault(MemtlyConfiguration.Gallery.UploadPeriod, "1970-01-01 00:00", gallery?.Id))?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            if (periods != null)
                            {
                                uploadActvated = false;

                                var now = DateTime.UtcNow;
                                foreach (var period in periods)
                                {
                                    var timeRanges = period?.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                    if (timeRanges != null && timeRanges.Length > 0)
                                    {
                                        var startDate = DateTime.Parse(timeRanges[0]).ToUniversalTime();

                                        if (timeRanges.Length == 2)
                                        {
                                            var endDate = DateTime.Parse(timeRanges[1]).ToUniversalTime();
                                            if (now >= startDate && now < endDate)
                                            {
                                                uploadActvated = true;
                                                break;
                                            }
                                        }
                                        else if (timeRanges.Length == 1 && now >= startDate)
                                        {
                                            uploadActvated = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            uploadActvated = true;
                        }
                    }

                    IDictionary<string, int> itemCounts;
                    if (gallery!.Type == GalleryType.Collection)
                    {
                        itemCounts = await _database.GetCollectionItemCount(term, userId, gallery?.Id, GalleryItemState.All, mediaType, orientation);
                    }
                    else
                    {
                        itemCounts = await _database.GetGalleryItemCount(term, userId, gallery?.Id, GalleryItemState.All, mediaType, orientation);
                    }

                    var galleryIdentifiers = gallery!.Type != GalleryType.Collection ? new Dictionary<int, GalleryIdentifierModel?>() { { gallery.Id, new GalleryIdentifierModel(gallery.Id, gallery.Identifier, gallery.Name) } } : items?.GroupBy(x => x.GalleryId)?.Select(x => new KeyValuePair<int, GalleryIdentifierModel?>(x.Key, _database.GetGalleryIdentifier(x.Key).Result))?.ToDictionary();
                    var model = new PhotoGallery()
                    {
                        Gallery = gallery,
                        SecretKey = gallery.SecretKey,
                        Images = items?.Select(x => {
                            var galleryIdentifier = galleryIdentifiers != null && galleryIdentifiers.ContainsKey(x.GalleryId) ? galleryIdentifiers[x.GalleryId] : new GalleryIdentifierModel(gallery.Id, gallery.Identifier, gallery.Name);
                            return new PhotoGalleryImage()
                            {
                                Id = x.Id,
                                GalleryId = galleryIdentifier!.Id,
                                GalleryName = galleryIdentifier!.Name,
                                Name = Path.GetFileName(x.Title),
                                UploadedBy = x.UploadedBy ?? "Unknown",
                                UploaderId = x.UserId,
                                UploaderEmailAddress = x.UploaderEmailAddress,
                                UploadDate = x.UploadedDate,
                                CaptureDate = x.DateTaken ?? x.UploadedDate,
                                ImagePath = $"/{Path.Combine(UploadsDirectory, galleryIdentifier!.Identifier).Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{(x!.State == GalleryItemState.Pending ? "Pending/" : string.Empty)}{Uri.EscapeDataString(x.Title)}",
                                ThumbnailPath = $"/{Path.Combine(ThumbnailsDirectory, galleryIdentifier!.Identifier).Remove(RootDirectory).Replace('\\', '/').TrimStart('/')}/{Uri.EscapeDataString(Path.GetFileNameWithoutExtension(x.Title))}.webp",
                                FallbackImagePath = $"/_content/Memtly.Core/images/{(x.MediaType == MediaType.Video ? "BrokenVideo" : "BrokenImage")}.webp",
                                Orientation = x.Orientation,
                                MediaType = x.MediaType,
                                State = x.State
                            };
                        })?.ToList(),
                        CurrentPage = currentPage,
                        ApprovedCount = itemCounts.ContainsKey("Approved") ? (int)itemCounts["Approved"] : 0,
                        PendingCount = itemCounts.ContainsKey("Pending") ? (int)itemCounts["Pending"] : 0,
                        UserApprovedCount = _identity.IsPrivilegedUser(User) || _identity.IsOwner(User, gallery!.Owner) ? (itemCounts.ContainsKey("Approved") ? (int)itemCounts["Approved"] : 0) : (itemCounts.ContainsKey("UserApproved") ? (int)itemCounts["UserApproved"] : 0),
                        UserPendingCount = _identity.IsPrivilegedUser(User) || _identity.IsOwner(User, gallery!.Owner) ? (itemCounts.ContainsKey("Pending") ? (int)itemCounts["Pending"] : 0) : (itemCounts.ContainsKey("UserPending") ? (int)itemCounts["UserPending"] : 0),
                        ItemsPerPage = itemsPerPage,
                        UploadActivated = uploadActvated,
                        ViewMode = (ViewMode)ViewBag.ViewMode,
                        GroupBy = galleryGroup,
                        OrderBy = galleryOrder,
                        Pagination = galleryOrder != GalleryOrder.Random,
                        LoadScripts = !partial
                    };

                    if (gallery.Id > 0 && userId > 0)
                    {
                        try
                        {
                            var galleryHistoryLimit = await _settings.GetOrDefault(MemtlyConfiguration.Account.GalleryHistoryLimit, 5);
                            await _database.AddGalleryHistory((int)userId, gallery.Id, gallery.SecretKey, limit: galleryHistoryLimit);
                        }
                        catch
                        {
                            _logger.LogWarning($"Failed to log gallery history for user '{userId}' on gallery '{gallery?.Id}'");
                        }
                    }

                    if (pagination)
                    {
                        return PartialView("~/Views/Gallery/Modes/Default.cshtml", model);
                    }
                    else if (partial)
                    {
                        return PartialView("~/Views/Gallery/GalleryWrapper.cshtml", model);
                    }
                    else
                    {
                        return View(model);
                    }
                }
            }

            return await ErrorResponse(ErrorCode.InvalidGalleryId);
        }

        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> UploadFileChunk([FromForm] MediaUploadRequest request)
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;

            try
            {
                if (request.CollectionId < 0)
                {
                    return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, _localizer["Invalid_Collection_Id"].Value));
                }

                if (request.GalleryId < 0)
                {
                    return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, _localizer["Invalid_Gallery_Id"].Value));
                }

                var galleryType = request.CollectionId > 0 ? GalleryType.Collection : GalleryType.Basic;

                var collection = request.CollectionId > 0 ? await _database.GetGallery(request.CollectionId) : null;
                if (request.CollectionId > 0 && collection == null)
                {
                    return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, _localizer["Invalid_Collection_Id"].Value));
                }

                var gallery = await _database.GetGallery(request.GalleryId);
                if (gallery != null)
                {
                    if (galleryType == GalleryType.Collection && (!string.IsNullOrWhiteSpace(collection?.SecretKey) && !string.Equals(collection?.SecretKey, request.SecretKey)))
                    {
                        return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, _localizer["Invalid_Secret_Key_Warning"].Value));
                    }
                    else if (galleryType != GalleryType.Collection && (!string.IsNullOrWhiteSpace(gallery?.SecretKey) && !string.Equals(gallery?.SecretKey, request.SecretKey)))
                    {
                        return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, _localizer["Invalid_Secret_Key_Warning"].Value));
                    }

                    try
                    {
                        if (request.File != null)
                        {
                            var extension = Path.GetExtension(request.File.FileName);
                            var maxGallerySize = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.MaxSizeMB, 1024L, collection?.Id ?? gallery.Id) * 1000000;
                            var maxFilesSize = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.MaxFileSizeMB, 50L, collection?.Id ?? gallery.Id) * 1000000;
                            var isDemoMode = await _settings.GetOrDefault(MemtlyConfiguration.IsDemoMode, false);

                            var allowedFileTypes = (await _settings.GetOrDefault(MemtlyConfiguration.Gallery.AllowedFileTypes, ".jpg,.jpeg,.png,.mp4,.mov", collection?.Id ?? gallery.Id)).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            if (!allowedFileTypes.Any(x => string.Equals(x.Trim('.'), extension.Trim('.'), StringComparison.OrdinalIgnoreCase)))
                            {
                                return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, $"{_localizer["File_Upload_Failed"].Value}. {_localizer["Invalid_File_Type"].Value}"));
                            }
                            else if (request.FileSize > maxFilesSize)
                            {
                                return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, $"{_localizer["File_Upload_Failed"].Value}. {_localizer["Max_File_Size"].Value} {maxFilesSize} bytes"));
                            }
                            else if ((_fileHelper.GetDirectorySize(Path.Combine(UploadsDirectory, gallery.Identifier)) + request.File.Length) > maxGallerySize)
                            {
                                return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, $"{_localizer["File_Upload_Failed"].Value}. {_localizer["Gallery_Full"].Value} {maxGallerySize} bytes"));
                            }
                            else if (await _settings.GetOrDefault(MemtlyConfiguration.Gallery.PreventDuplicates, true, collection?.Id ?? gallery.Id) && (string.IsNullOrWhiteSpace(request.FileChecksum) || await _database.GetGalleryItemByChecksum(gallery.Id, request.FileChecksum) != null))
                            {
                                return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, $"{_localizer["File_Upload_Failed"].Value}. {_localizer["Duplicate_Item_Detected"].Value}"));
                            }
                            else
                            {
                                string uploadedBy = HttpContext.Session.GetString(SessionKey.Viewer.Identity)?.Trim() ?? "Anonymous";
                                string uploaderEmail = HttpContext.Session.GetString(SessionKey.Viewer.EmailAddress)?.Trim() ?? "Anonymous";

                                var loggedInUserId = _identity.GetUserId(User);
                                var loggedInUser = loggedInUserId > 0 ? await _database.GetUser(loggedInUserId) : null;
                                if (loggedInUser != null)
                                {
                                    uploadedBy = $"{loggedInUser.Firstname} {loggedInUser.Lastname}".Trim();
                                    if (string.IsNullOrWhiteSpace(uploadedBy))
                                    {
                                        uploadedBy = loggedInUser.Username;
                                    }

                                    uploaderEmail = loggedInUser?.Email?.Trim() ?? string.Empty;
                                }

                                var galleryOwner = await _database.GetUser(collection?.Owner ?? gallery.Owner);
                                var requiresReview = galleryOwner!.CanUseFeature(FeaturePermissions.RequireGalleryItemReview) && await _settings.GetOrDefault(MemtlyConfiguration.Gallery.RequireReview, true, collection?.Id ?? gallery.Id);
                                var galleryPath = requiresReview ? Path.Combine(UploadsDirectory, gallery.Identifier, "Pending") : Path.Combine(UploadsDirectory, gallery.Identifier);

                                _fileHelper.CreateDirectoryIfNotExists(galleryPath);
                                
                                var finalFileName = _fileHelper.SanitizeFilename($"{(!string.IsNullOrWhiteSpace(uploadedBy) ? $"{uploadedBy.Replace(" ", "_")}-" : string.Empty)}{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}");
                                var finalFilePath = Path.Combine(galleryPath, finalFileName);

                                if (request.TotalChunks == 1 || isDemoMode)
                                {
                                    if (!isDemoMode)
                                    {
                                        await _fileHelper.SaveFile(request.File, finalFilePath, FileMode.Create);
                                    }
                                    else
                                    {
                                        var mediaType = _imageHelper.GuessMediaTypeFromExtension(request.File.FileName);
                                        System.IO.File.Copy(Path.Combine(AssetsDirectory, (mediaType == MediaType.Video ? $"DemoVideo.mp4" : $"DemoImage.png")), finalFilePath, true);
                                    }
                                }
                                else
                                {
                                    _fileHelper.CreateDirectoryIfNotExists(TempDirectory);

                                    var fileName = _fileHelper.SanitizeFilename($"{request.UploadId}_{request.ChunkIndex + 1}.part");
                                    var filePath = Path.Combine(TempDirectory, fileName);

                                    await _fileHelper.SaveFile(request.File, filePath, FileMode.Create);

                                    var uploadedChunks = _fileHelper.GetFiles(TempDirectory, $"{request.UploadId}_*.part", SearchOption.TopDirectoryOnly);
                                    if (uploadedChunks.Count() == request.TotalChunks)
                                    {
                                        await using (var output = System.IO.File.Create(finalFilePath))
                                        {
                                            for (var i = 0; i < request.TotalChunks; i++)
                                            {
                                                var chunkFileName = _fileHelper.SanitizeFilename($"{request.UploadId}_{i + 1}.part");
                                                var chunkPath = Path.Combine(TempDirectory, chunkFileName);
                                                await using var chunkStream = System.IO.File.OpenRead(chunkPath);
                                                await chunkStream.CopyToAsync(output);
                                            }
                                        }

                                        if (!string.IsNullOrWhiteSpace(finalFilePath) && _fileHelper.FileExists(finalFilePath))
                                        {
                                            foreach (var part in uploadedChunks)
                                            {
                                                _fileHelper.DeleteFileIfExists(part);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Response.StatusCode = (int)HttpStatusCode.OK;
                                        return Json(new MediaUploadSuccessResponse(request.RequestId, request.UploadId));
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(finalFilePath) && _fileHelper.FileExists(finalFilePath))
                                {
                                    var imageOrientation = ImageOrientation.Unknown;

                                    try
                                    {
                                        var thumbnailPath = Path.Combine(ThumbnailsDirectory, gallery.Identifier);

                                        _fileHelper.CreateDirectoryIfNotExists(ThumbnailsDirectory);
                                        _fileHelper.CreateDirectoryIfNotExists(thumbnailPath);

                                        var savePath = Path.Combine(thumbnailPath, $"{Path.GetFileNameWithoutExtension(finalFilePath)}.webp");
                                        var thumbnailSize = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Thumbnails.Size, 720, gallery.Id);
                                        
                                        await _imageHelper.GenerateThumbnail(finalFilePath, savePath, thumbnailSize);
                                        
                                        imageOrientation = _imageHelper.GetOrientation(savePath);
                                    }
                                    catch (Exception ex) 
                                    {
                                        _logger.LogWarning(ex, $"{_localizer["Failed_To_Generate_Thumbnail"].Value} - '{finalFilePath}' - {ex?.Message}");
                                    }

                                    var item = await _database.AddGalleryItem(new GalleryItemModel()
                                    {
                                        GalleryId = gallery.Id,
                                        GalleryName = gallery.Name,
                                        UserId = loggedInUser?.Id,
                                        Title = finalFileName,
                                        UploadedBy = uploadedBy,
                                        UploaderEmailAddress = uploaderEmail,
                                        UploadedDate = DateTimeOffset.UtcNow,
                                        DateTaken = _imageHelper.GetExifCreationDateTaken(finalFilePath) ?? await _fileHelper.GetCreationDatetime(finalFilePath),
                                        Checksum = request.FileChecksum,
                                        MediaType = _imageHelper.GetMediaType(finalFilePath),
                                        Orientation = imageOrientation,
                                        State = requiresReview ? GalleryItemState.Pending : GalleryItemState.Approved,
                                        FileSize = request.FileSize,
                                    });

                                    Response.StatusCode = (int)HttpStatusCode.OK;
                                    return Json(new MediaUploadCompleteResponse(request.RequestId, request.UploadId));
                                }

                                return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, _localizer["No_Files_For_Upload"].Value));
                            }
                        }
                        else
                        {
                            return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, _localizer["No_Files_For_Upload"].Value));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"{_localizer["Save_To_Gallery_Failed"].Value} - {ex?.Message}");
                    }
                }
                else
                {
                    return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, _localizer["Gallery_Does_Not_Exist"].Value));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_localizer["Image_Upload_Failed"].Value} - {ex?.Message}");
            }

            return Json(new MediaUploadFailureResponse(request.RequestId, request.UploadId, _localizer["Image_Upload_Failed"].Value));
        }

        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> UploadCompleted([FromForm] MediaBatchUploadRequest request)
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;

            try
            {
                if (request.CollectionId < 0)
                {
                    return Json(new MediaBatchUploadFailureResponse(request.RequestId, _localizer["Invalid_Collection_Id"].Value));
                }

                if (request.GalleryId < 0)
                {
                    return Json(new MediaBatchUploadFailureResponse(request.RequestId, _localizer["Invalid_Gallery_Id"].Value));
                }

                var galleryType = request.CollectionId > 0 ? GalleryType.Collection : GalleryType.Basic;

                var collection = request.CollectionId > 0 ? await _database.GetGallery(request.CollectionId) : null;
                if (request.CollectionId > 0 && collection == null)
                {
                    return Json(new MediaBatchUploadFailureResponse(request.RequestId, _localizer["Invalid_Collection_Id"].Value));
                }

                var gallery = await _database.GetGallery(request.GalleryId);
                if (gallery != null)
                {
                    if (galleryType == GalleryType.Collection && (!string.IsNullOrWhiteSpace(collection?.SecretKey) && !string.Equals(collection?.SecretKey, request.SecretKey)))
                    {
                        return Json(new MediaBatchUploadFailureResponse(request.RequestId, _localizer["Invalid_Secret_Key_Warning"].Value));
                    }
                    else if (galleryType != GalleryType.Collection && (!string.IsNullOrWhiteSpace(gallery?.SecretKey) && !string.Equals(gallery?.SecretKey, request.SecretKey)))
                    {
                        return Json(new MediaBatchUploadFailureResponse(request.RequestId, _localizer["Invalid_Secret_Key_Warning"].Value));
                    }

                    var galleryOwner = await _database.GetUser(collection?.Owner ?? gallery.Owner);
                    var requiresReview = galleryOwner!.CanUseFeature(FeaturePermissions.RequireGalleryItemReview) && await _settings.GetOrDefault(MemtlyConfiguration.Gallery.RequireReview, true, collection?.Id ?? gallery.Id);

                    string uploadedBy = HttpContext.Session.GetString(SessionKey.Viewer.Identity)?.Trim() ?? "Anonymous";

                    var loggedInUserId = _identity.GetUserId(User);
                    var loggedInUser = loggedInUserId > 0 ? await _database.GetUser(loggedInUserId) : null;
                    if (loggedInUser != null)
                    {
                        uploadedBy = $"{loggedInUser.Firstname} {loggedInUser.Lastname}".Trim();
                        if (string.IsNullOrWhiteSpace(uploadedBy))
                        {
                            uploadedBy = loggedInUser.Username;
                        }
                    }

                    if (requiresReview && await _settings.GetOrDefault(MemtlyConfiguration.Alerts.PendingReview, true))
                    {
                        await _notificationHelper.Send(_localizer["New_Items_Pending_Review"].Value, $"{request.UploadCount} new item(s) have been uploaded to gallery '{gallery.Name}' by '{(!string.IsNullOrWhiteSpace(uploadedBy) ? uploadedBy : "Anonymous")}' and are awaiting your review.", _urlHelper.GenerateBaseUrl(HttpContext?.Request, "/Account"));
                    }

                    Response.StatusCode = (int)HttpStatusCode.OK;
                    return Json(new MediaBatchUploadSuccessResponse(request.RequestId, requiresReview, new MediaBatchUploadCounters()
                    {
                        Total = collection?.TotalItems ?? gallery?.TotalItems ?? 0,
                        Approved = collection?.ApprovedItems ?? gallery?.ApprovedItems ?? 0,
                        Pending = collection?.PendingItems ?? gallery?.PendingItems ?? 0
                    }));
                }
                else
                {
                    return Json(new MediaBatchUploadFailureResponse(request.RequestId, _localizer["Gallery_Does_Not_Exist"].Value));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_localizer["Image_Upload_Failed"].Value} - {ex?.Message}");
            }

            return Json(new MediaBatchUploadFailureResponse(request.RequestId, _localizer["Image_Upload_Failed"].Value));
        }

        [HttpPost]
        [RequestTimeout("timeout_1h")]
        public async Task<IActionResult> DownloadGallery(int id, string? secretKey, string? group, List<string>? fileFilter)
        {
            try
            {
                var userId = _identity.GetUserId(User);

                var gallery = await _database.GetGallery(id);
                if (gallery != null)
                {
                    secretKey = secretKey ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(gallery.SecretKey) && !secretKey.Equals(gallery.SecretKey))
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        _logger.LogWarning($"{_localizer["Failed_Download_Gallery"].Value} - Gallery Id: {id}, Group: '{group}', File Filter: '{string.Join(',', fileFilter ?? [])}' - Invalid secret key provided: '{secretKey}'");
                        return Json(new { success = false, message = _localizer["Failed_Download_Gallery_Invalid_Key"].Value });
                    }

                    if (await _settings.GetOrDefault(MemtlyConfiguration.Gallery.Download, true, gallery?.Id) || _identity.IsOwner(User, gallery!.Owner) || _identity.IsPrivilegedUser(User))
                    {
                        var galleryDirs = new List<string>();

                        if (gallery!.Identifier.Equals(SystemGalleries.AllGallery, StringComparison.OrdinalIgnoreCase))
                        {
                            var galleryDir = UploadsDirectory;
                            if (_fileHelper.DirectoryExists(galleryDir))
                            {
                                galleryDirs.Add(galleryDir);
                            }
                            else
                            {
                                _logger.LogWarning($"{_localizer["Failed_Download_Gallery"].Value} - Gallery Id: {id}, Group: '{group}', File Filter: '{string.Join(',', fileFilter ?? [])}' - Failed to find gallery directory at '{galleryDir}'");
                            }
                        }
                        else if (gallery!.Type != GalleryType.Collection)
                        {
                            var galleryDir = Path.Combine(UploadsDirectory, gallery!.Identifier);
                            if (_fileHelper.DirectoryExists(galleryDir))
                            {
                                galleryDirs.Add(galleryDir);
                            }
                            else
                            {
                                _logger.LogWarning($"{_localizer["Failed_Download_Gallery"].Value} - Gallery Id: {id}, Group: '{group}', File Filter: '{string.Join(',', fileFilter ?? [])}' - Failed to find gallery directory at '{galleryDir}'");
                            }
                        }
                        else if (gallery!.Type == GalleryType.Collection)
                        {
                            if (gallery?.CollectionItems != null)
                            {
                                foreach (var galleryId in gallery.CollectionItems)
                                {
                                    var gal = await _database.GetGallery(galleryId);
                                    if (gal != null)
                                    {
                                        var galleryDir = Path.Combine(UploadsDirectory, gal!.Identifier);
                                        if (_fileHelper.DirectoryExists(galleryDir))
                                        {
                                            galleryDirs.Add(galleryDir);
                                        }
                                        else
                                        {
                                            _logger.LogWarning($"{_localizer["Failed_Download_Gallery"].Value} - Gallery Id: {id}, Group: '{group}', File Filter: '{string.Join(',', fileFilter ?? [])}' - Failed to find gallery directory at '{galleryDir}'");
                                        }
                                    }
                                }
                            }
                        }

                        if (galleryDirs != null && galleryDirs.Any())
                        {
                            fileFilter = fileFilter ?? new List<string>();

                            var filterDropItems = gallery!.Type == GalleryType.Drop && _identity.IsBasicUser(User) && userId != gallery!.Owner;
                            if (filterDropItems && string.IsNullOrWhiteSpace(group))
                            {
                                group = $"{(int)GalleryGroup.None}|DropItemsOnly|{GalleryItemState.All}";
                            }

                            if (!string.IsNullOrWhiteSpace(group))
                            {
                                try
                                {
                                    var groupParts = group.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                    if (groupParts != null && groupParts.Length == 3)
                                    {
                                        var tempFilter = fileFilter;
                                        fileFilter = new List<string>();

                                        GalleryItemState state = GalleryItemState.Approved;

                                        var showPendingUploads = await _settings.GetOrDefault(MemtlyConfiguration.Gallery.ShowPendingUploads, true, gallery?.Id);
                                        if (showPendingUploads)
                                        {
                                            if (_identity.IsValid(User))
                                            {
                                                foreach (GalleryItemState s in Enum.GetValues(typeof(GalleryItemState)))
                                                {
                                                    if (string.Equals(groupParts[2], s.ToString(), StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        state = s;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        IEnumerable<GalleryItemModel>? galleryItems;
                                        if (gallery!.Type == GalleryType.Collection && !gallery!.Identifier.Equals(SystemGalleries.AllGallery, StringComparison.OrdinalIgnoreCase))
                                        {
                                            galleryItems = await _database.GetCollectionItems(string.Empty, null, id, state);
                                        }
                                        else
                                        {
                                            galleryItems = await _database.GetGalleryItems(string.Empty, null, id, state);
                                        }

                                        if (_identity.IsBasicUser(User) && !_identity.IsOwner(User, gallery!.Owner))
                                        {
                                            if (gallery.Type == GalleryType.Drop)
                                            {
                                                galleryItems = galleryItems?.Where(x => x.UserId != null && x.UserId == userId);
                                            }
                                            else
                                            {
                                                galleryItems = galleryItems?.Where(x => x.State == GalleryItemState.Approved || (x.State == GalleryItemState.Pending && x.UserId != null && x.UserId == userId));
                                            }
                                        }

                                        if (filterDropItems)
                                        {
                                            galleryItems = galleryItems.Where(x => x.UserId != null && x.UserId == userId).ToList();
                                        }

                                        if (((int)GalleryGroup.None).ToString().Equals(groupParts[0]))
                                        {
                                            fileFilter.AddRange(galleryItems.Select(x => x.Title).Where(x => tempFilter == null || !tempFilter.Any() || tempFilter.Contains(x)));
                                        }
                                        else
                                        {
                                            fileFilter.AddRange(string.Empty);

                                            foreach (GalleryGroup type in Enum.GetValues(typeof(GalleryGroup)))
                                            {
                                                if (((int)type).ToString().Equals(groupParts[0]))
                                                {
                                                    try
                                                    {
                                                        IEnumerable<IGrouping<string, GalleryItemModel>>? filtered = null;
                                                        switch (type)
                                                        {
                                                            case GalleryGroup.Gallery:
                                                                filtered = galleryItems?.GroupBy(x => x.GalleryName);
                                                                break;
                                                            case GalleryGroup.DateUploaded:
                                                                filtered = galleryItems?.GroupBy(x => x.UploadedDate.ToLocalTime().ToString("dddd, d MMMM yyyy"));
                                                                break;
                                                            case GalleryGroup.DateTaken:
                                                                filtered = galleryItems?.GroupBy(x => (x.DateTaken ?? x.UploadedDate).ToLocalTime().ToString("dddd, d MMMM yyyy"));
                                                                break;
                                                            case GalleryGroup.MediaType:
                                                                filtered = galleryItems?.GroupBy(x => x.MediaType.ToString());
                                                                break;
                                                            case GalleryGroup.Uploader:
                                                                filtered = galleryItems?.GroupBy(x => x.UploadedBy ?? "Anonymous");
                                                                break;
                                                        }

                                                        if (filtered != null)
                                                        {
                                                            foreach (var f in filtered)
                                                            {
                                                                if (f.Key.Equals(groupParts[1]))
                                                                {
                                                                    if (f.Any())
                                                                    {
                                                                        fileFilter.AddRange(f.Select(x => x.Title).Where(x => tempFilter == null || !tempFilter.Any() || tempFilter.Contains(x)));
                                                                    }

                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    catch { }

                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"{_localizer["Failed_Download_Gallery"].Value} - Gallery Id: {id}, Group: '{group}', File Filter: '{string.Join(',', fileFilter ?? [])}' - Invalid group format detected");
                                        return Json(new { success = false, message = _localizer["Failed_Download_Gallery"].Value });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, $"{_localizer["Failed_Download_Gallery"].Value} - Gallery Id: {id}, Group: '{group}', File Filter: '{string.Join(',', fileFilter ?? [])}' - Failed to parse gallery download listing - {ex?.Message}");
                                }
                            }

                            var archieveName = $"{gallery!.Identifier ?? "Memtly"}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.zip";

                            var listing = new List<ZipListing>();

                            if (_identity.IsOwner(User, gallery.Owner) || _identity.IsPrivilegedUser(User))
                            {
                                var scanners = new List<ZipListingScanner>();

                                foreach (var galleryDir in galleryDirs)
                                {
                                    scanners.Add(new ZipListingScanner("Approved", galleryDir, SearchOption.TopDirectoryOnly));
                                    scanners.Add(new ZipListingScanner("Pending", Path.Combine(galleryDir, "Pending"), SearchOption.AllDirectories));
                                    scanners.Add(new ZipListingScanner("Rejected", Path.Combine(galleryDir, "Rejected"), SearchOption.AllDirectories));
                                }

                                foreach (var scanner in scanners)
                                {
                                    try
                                    {
                                        var files = Directory.GetFiles(scanner.Path, "*", scanner.SearchOption);
                                        if (fileFilter != null && fileFilter.Any())
                                        {
                                            files = files.Where(x => fileFilter.Exists(y => Path.GetFileName(y).Equals(Path.GetFileName(x), StringComparison.OrdinalIgnoreCase))).ToArray();
                                        }

                                        if (files != null && files.Any())
                                        {
                                            listing.Add(new ZipListing(scanner.Path, files, scanner.Name));
                                        }
                                    }
                                    catch { }
                                }
                            }
                            else
                            {
                                foreach (var galleryDir in galleryDirs)
                                {
                                    var files = Directory.GetFiles(galleryDir, "*", SearchOption.TopDirectoryOnly);
                                    if (fileFilter != null && fileFilter.Any())
                                    {
                                        files = files.Where(x => fileFilter.Exists(y => Path.GetFileName(y).Equals(Path.GetFileName(x), StringComparison.OrdinalIgnoreCase))).ToArray();
                                    }

                                    if (files != null && files.Any())
                                    {
                                        listing.Add(new ZipListing(galleryDir, files));
                                    }
                                }
                            }

                            if (listing != null && listing.Count > 0)
                            {
                                if (listing.GroupBy(x => x.Directory).Count() == 1)
                                {
                                    listing = listing.Select(x => new ZipListing(x.SourcePath, x.Files)).ToList();
                                }

                                return await ZipFileResponse(archieveName, listing);
                            }
                            else
                            {
                                _logger.LogWarning($"{_localizer["Failed_Download_Gallery"].Value} - Gallery Id: {id}, Group: '{group}', File Filter: '{string.Join(',', fileFilter ?? [])}' - Listing was empty'");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"{_localizer["Failed_Download_Gallery"].Value} - Gallery Id: {id}, Group: '{group}', File Filter: '{string.Join(',', fileFilter ?? [])}' - Failed to find gallery directories'");
                        }
                    }
                    else
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        _logger.LogWarning($"{_localizer["Download_Gallery_Not_Allowed"].Value} - {id}");
                        return Json(new { success = false, message = _localizer["Download_Gallery_Not_Allowed"].Value });
                    }
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    _logger.LogWarning($"{_localizer["Failed_Download_Gallery"].Value} - Gallery Id: {id}, Gallery Identifier: '{gallery?.Identifier}', Gallery Name: '{gallery?.Name}'");
                    return Json(new { success = false, message = _localizer["Failed_Download_Gallery"].Value });
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogError(ex, $"{_localizer["Failed_Download_Gallery"].Value} - {ex?.Message}");
            }

            return Json(new { success = false });
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public string GenerateSecretKey()
        {
            return PasswordHelper.GenerateGallerySecretKey();
        }

        [HttpPost]
        [RequiresRole(CollectionPermission = CollectionPermissions.View)]
        [Route("Collection/Items")]
        public async Task<IActionResult> GetCollectionItems(int? collectionId = null)
        {
            var items = new List<CollectionSelectItem>();

            if (_identity.IsValid(User))
            {
                var userId = _identity.GetUserId(User);

                var galleries = new List<GalleryModel>();

                galleries.AddRange(await _database.GetGalleries(userId, type: GalleryType.Basic));
                galleries.AddRange(await _database.GetGalleries(userId, type: GalleryType.Drop));
                
                if (galleries != null && galleries.Any())
                {
                    var collections = collectionId != null ? await _database.GetCollections(userId, collectionId) : new List<GalleryCollectionModel>();
                    if (collections != null)
                    {
                        items = galleries
                            .OrderBy(g => g.Name.ToUpper())
                            .Select(g => new CollectionSelectItem(g.Id, g.Name, collections.Any(c => c.GalleryId == g.Id)))
                            .ToList();
                    }
                }
            }

            return Json(new { items });
        }

        [HttpPost]
        [RequiresRole(CollectionPermission = CollectionPermissions.View)]
        [Route("Collection/Galleries")]
        public async Task<IActionResult> GetCollectionGalleries(int collectionId)
        {
            var items = new List<CollectionSelectItem>();

            var galleries = await _database.GetGalleriesByCollectionId(collectionId);
            if (galleries != null)
            {
                items = galleries
                    .OrderBy(g => g.Name.ToUpper())
                    .Select(g => new CollectionSelectItem(g.Id, g.Name))
                    .ToList();
            }

            return Json(new { items });
        }

        [HttpPost]
        [RequiresRole(CollectionPermission = CollectionPermissions.View)]
        [Route("Gallery/Shares")]
        public async Task<IActionResult> GetGalleryShareUsers(int galleryId)
        {
            var items = new List<ShareSelectItem>();

            if (_identity.IsValid(User))
            {
                var userId = _identity.GetUserId(User);

                var shares = await _database.GetGalleryShareUsers(galleryId);
                if (shares != null && shares.Any())
                {
                    items = shares
                        .OrderBy(s => s.UserName.ToUpper())
                        .Select(s => new ShareSelectItem(s.UserId, s.UserName, true))
                        .ToList();
                }
            }

            return Json(new { items });
        }
    }
}