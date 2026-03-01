namespace Memtly.Core.Constants
{
    public class ProtectedValues
    {
        public static readonly string[] GalleryNames = [ SystemGalleries.AllGallery ];

        public static bool IsProtectedGalleryName(string name) 
        {
            return GalleryNames.Any(x => x.ToLower().Equals(name?.Trim().ToLower()));
        }
    }
}