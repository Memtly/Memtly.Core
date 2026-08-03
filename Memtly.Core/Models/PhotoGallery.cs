using System.Text.Json.Serialization;
using Memtly.Core.Enums;
using Memtly.Core.Models.Database;

namespace Memtly.Core.Models
{
    public class PhotoGallery
    {
        public PhotoGallery()
        {
        }

        public GalleryModel? Gallery { get; set; }
        public string? SecretKey { get; set; }
        public ViewMode ViewMode { get; set; } = ViewMode.Default;
        public GalleryGroup GroupBy { get; set; } = GalleryGroup.None;
        public GalleryOrder OrderBy { get; set; } = GalleryOrder.Descending;
        public int ApprovedCount { get; set; } = 0;
        public int PendingCount { get; set; } = 0;
        public int ItemsPerPage { get; set; } = 50;
        public int CurrentPage { get; set; } = 1;
        public bool Pagination { get; set; } = true;
        public bool LoadScripts { get; set; } = true;
        public int TotalCount
        {
            get
            {
                return this.ApprovedCount + this.PendingCount;
            }
        }
        public List<PhotoGalleryImage>? Images { get; set; }
        public bool UploadActivated { get; set; } = false;
    }

    public class PhotoGalleryImage
    {
        public PhotoGalleryImage()
        { 
        }

        public int Id { get; set; }
        public int? GalleryId { get; set; }
        public string? GalleryName { get; set; }
        public string? Name { get; set; }
        public int? UploaderId { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public string? UploaderEmailAddress { get; set; }
        public DateTimeOffset? UploadDate { get; set; }
        public DateTimeOffset? CaptureDate { get; set; }
        public string? ImagePath { get; set; }
        public string? ThumbnailPath { get; set; }
        public MediaType MediaType { get; set; }
        public GalleryItemState State { get; set; }
    }

    public class PhotoGalleryImageLikes
    {
        public PhotoGalleryImageLikes()
        {
        }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("can_like")]
        public bool CanUserLike { get; set; } = false;

        [JsonPropertyName("has_liked")]
        public bool HasUserLiked { get; set; } = false;

        [JsonPropertyName("count")]
        public long Count { get; set; } = 0;
    }
}