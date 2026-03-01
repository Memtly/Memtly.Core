using Memtly.Core.Helpers;

namespace Memtly.Core.Models.Database
{
    public class GalleryModel
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = GalleryHelper.GenerateGalleryIdentifier();
        public string Name { get; set; } = "Unknown";
        public string? SecretKey { get; set; }
        public int TotalItems { get; set; }
        public int ApprovedItems { get; set; }
        public int PendingItems { get; set; }
        public long TotalGallerySize { get; set; }
        public int Owner { get; set; }
        public string OwnerName { get; set; } = "Unknown";

        public string CalculateUsage(long maxSizeMB = long.MaxValue)
        {
            return ((double)(TotalGallerySize / (double)(maxSizeMB * 1000000L))).ToString("0.00%");
        }
    }
}