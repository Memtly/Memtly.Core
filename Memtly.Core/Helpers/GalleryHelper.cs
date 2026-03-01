using System.Text.RegularExpressions;

namespace Memtly.Core.Helpers
{
    public class GalleryHelper
    {
        public static string GenerateGalleryIdentifier()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower();
        }

        public static bool IsValidGalleryIdentifier(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, "^(all|default|[a-z0-9]{32})$", RegexOptions.Compiled);
        }
    }
}