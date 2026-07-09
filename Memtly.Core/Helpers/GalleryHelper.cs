using System.Text.RegularExpressions;

namespace Memtly.Core.Helpers
{
    public class GalleryHelper
    {
        public static string GenerateGalleryIdentifier()
        {
            var maxLength = 32;

            var identifier = Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower();
            if (identifier.Length > maxLength)
            {
                identifier = identifier.Substring(0, maxLength);
            }

            return identifier;
        }

        public static bool IsValidGalleryIdentifier(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, "^(all|default|[a-z0-9]{32})$", RegexOptions.Compiled);
        }
    }
}