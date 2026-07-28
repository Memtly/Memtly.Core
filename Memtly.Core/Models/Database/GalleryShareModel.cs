using Memtly.Core.Enums;

namespace Memtly.Core.Models.Database
{
    public class GalleryShareModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "Unknown";
        public int GalleryId { get; set; }
        public string GalleryIdentifier { get; set; } = "Unknown";
        public string GalleryName { get; set; } = "Unknown";
        public string GalleryOwnerName { get; set; } = "Unknown";
        public GalleryType GalleryType { get; set; } = GalleryType.Basic;
        public string? SecretKey { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}