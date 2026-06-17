namespace Memtly.Core.EntityFramework.Models
{
    public class GalleryCollection
    {
        public int Id { get; set; }
        public int? CollectionId { get; set; }
        public Gallery? Collection { get; set; }
        public int? GalleryId { get; set; }
        public Gallery? Gallery { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}