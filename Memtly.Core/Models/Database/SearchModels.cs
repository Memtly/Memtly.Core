using Memtly.Core.Enums;

namespace Memtly.Core.Models.Database
{
    public class SearchModels
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = int.MaxValue;
    }

    public class BasicTermSearch : SearchModels
    {
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class GalleryItemSearch : BasicTermSearch
    {
        public int? UserId { get; set; } = null;
        public List<int>? CollectionIds { get; set; } = null;
        public List<int>? GalleryIds { get; set; } = null;
        public GalleryItemStateFilter ItemState { get; set; } = new GalleryItemStateFilter();
        public MediaType MediaType { get; set; } = MediaType.All;
        public ImageOrientation ImageOrientation { get; set; } = ImageOrientation.All;
        public GalleryGroup GroupBy { get; set; } = GalleryGroup.None;
        public GalleryOrder OrderBy { get; set; } = GalleryOrder.Descending;
        public List<string>? AllowedFileExtensions { get; set; } = null;
    }

    public class GalleryItemStateFilter
    {
        public ItemOwner Pending { get; set; } = ItemOwner.None;
        public ItemOwner Approved { get; set; } = ItemOwner.None;
    }

    public enum ItemOwner
    {
        All,
        UserOnly,
        None
    }
}