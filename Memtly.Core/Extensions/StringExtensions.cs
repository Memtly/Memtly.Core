namespace Memtly.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Replace(this string value, string[] oldChars, string newChar)
        {
            foreach (var s in oldChars)
            {
                if (!string.Equals(s, newChar))
                { 
                    value = value.Replace(s, newChar);
                }
            }

            return value;
        }

        public static string Remove(this string value, string needle)
        {
            return value.Replace(needle, string.Empty);
        }
    }
}