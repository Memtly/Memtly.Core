namespace Memtly.Core.EntityFramework.Models
{
    public class GalleryHistory
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public int? GalleryId { get; set; }
        public Gallery? Gallery { get; set; }
        public string? SecretKey { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}