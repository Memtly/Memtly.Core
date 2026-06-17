namespace Memtly.Core.Models.Database
{
    public class GalleryCollectionModel
    {
        public GalleryCollectionModel()
            : this(0, 0, 0, new DateTime(0, DateTimeKind.Utc))
        {
        }

        public GalleryCollectionModel(int id, int collectionId, int galleryId, DateTimeOffset timestamp)
        {
            Id = id;
            CollectionId = collectionId;
            GalleryId = galleryId;
            CreatedAt = timestamp;
        }

        public int Id { get; set; }
        public int CollectionId { get; set; }
        public int GalleryId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}