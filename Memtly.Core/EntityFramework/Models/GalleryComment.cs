namespace Memtly.Core.EntityFramework.Models
{
    public class GalleryComment
    {
        public int Id { get; set; }
        public int? GalleryItemId { get; set; }
        public GalleryItem? GalleryItem { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public string Value { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}