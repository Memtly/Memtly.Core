using Memtly.Core.Enums;

namespace Memtly.Core.Models.Database
{
    public class GalleryItemModel
    {
        public GalleryItemModel()
            : this(0, 0, string.Empty, null, string.Empty, null, null, new DateTime(0, DateTimeKind.Utc), null, null, MediaType.Unknown, ImageOrientation.Unknown, GalleryItemState.Pending, 0)
        {
        }

        public GalleryItemModel(int id, int galleryId, string galleryName, int? userId, string title, string? uploadedBy, string? uploaderEmailAddress, DateTimeOffset uploadedDate, DateTimeOffset? dateTaken, string? checksum, MediaType mediaType, ImageOrientation orientation, GalleryItemState state, long file_size)
        {
            Id = id;
            GalleryId = galleryId;
            GalleryName = galleryName;
            UserId = userId;
            Title = title;
            UploadedBy = uploadedBy;
            UploaderEmailAddress = uploaderEmailAddress;
            UploadedDate = uploadedDate;
            DateTaken = dateTaken;
            Checksum = checksum;
            MediaType = mediaType;
            Orientation = orientation;
            State = state;
            FileSize = file_size;
        }

        public int Id { get; set; }
        public int GalleryId { get; set; }
        public string GalleryName { get; set; }
        public int? UserId { get; set; }
        public string Title { get; set; }
        public string? UploadedBy { get; set; }
        public string? UploaderEmailAddress { get; set; }
        public DateTimeOffset UploadedDate { get; set; }
        public DateTimeOffset? DateTaken { get; set; }
        public string? Checksum { get; set; }
        public MediaType MediaType { get; set; } = MediaType.Unknown;
        public ImageOrientation Orientation { get; set; } = ImageOrientation.Unknown;
        public GalleryItemState State { get; set; } = GalleryItemState.Unknown;
        public long FileSize { get; set; }
    }
}