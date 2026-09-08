namespace Memtly.Core.Models.Database
{
    public class GalleryItemCommentModel
    {
        public GalleryItemCommentModel()
            : this(0, 0, 0, 0, string.Empty, string.Empty, new DateTime(0, DateTimeKind.Utc))
        {
        }

        public GalleryItemCommentModel(int id, int galleryItemId, int galleryId, int userId, string username, string value, DateTimeOffset timestamp)
        {
            Id = id;
            GalleryItemId = galleryItemId;
            GalleryId = galleryId;
            UserId = userId;
            Username = username;
            Value = value;
            Timestamp = timestamp;
        }

        public int Id { get; set; }
        public int GalleryItemId { get; set; }
        public int GalleryId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Value { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public string TimestampString 
        {
            get
            {
                var sincePost = (DateTimeOffset.UtcNow - this.Timestamp);
                if (sincePost.TotalMinutes < 1)
                {
                    return "Just_Now";
                }
                else if (sincePost.TotalMinutes < 10)
                {
                    return "Moments_Ago";
                }
                else if (sincePost.TotalDays < 1)
                {
                    return this.Timestamp.ToString("h:mm tt");
                }
                else
                {
                    return this.Timestamp.ToString("dd/MM/yyyy h:mm tt");
                }
            }
        }

        public string TimestampStringFull 
        {
            get
            {
                return this.Timestamp.ToString("dd/MM/yyyy hh:mm tt");
            }
        }
    }
}