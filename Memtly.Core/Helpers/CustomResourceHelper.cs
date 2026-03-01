namespace Memtly.Core.Helpers
{
    public class CustomResourceHelper
    {
        public static string GenerateCustomResourceIdentifier()
        {
            return GalleryHelper.GenerateGalleryIdentifier();
        }

        public static bool IsValidGalleryIdentifier(string? value)
        {
            return GalleryHelper.IsValidGalleryIdentifier(value);
        }
    }
}