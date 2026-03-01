using System.Text;

namespace Memtly.Core.Helpers
{
    public class EncodingHelper
    {
        public static string Base64Encode(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            }

            return string.Empty;
        }

        public static string Base64Decode(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(value));
            }

            return string.Empty;
        }
    }
}