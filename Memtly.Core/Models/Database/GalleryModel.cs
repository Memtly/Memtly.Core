using Memtly.Core.Enums;
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
        public GalleryType Type { get; set; } = GalleryType.Basic;
        public List<int>? CollectionItems { get; set; }

        public string CalculateUsage(long maxSizeMB = long.MaxValue)
        {
            return ((double)(TotalGallerySize / (double)((maxSizeMB * (CollectionItems?.Count ?? 1)) * 1000000L))).ToString("0.00%");
        }
    }
}