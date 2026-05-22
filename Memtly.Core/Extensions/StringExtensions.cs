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

        public static string ToHexColor(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    var parts = value.Split(',').Select(p => p.Trim()).ToArray();
                    if (parts.Length >= 3 
                        && int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int g) && int.TryParse(parts[2], out int b)
                        && r >= 0 && r <= 255 && g >= 0 && g <= 255 && b >= 0 && b <= 255)
                    {
                        return $"#{r:X2}{g:X2}{b:X2}";
                    }
                }
                catch { }
            }
                
            return "#000000";
        }

        public static string ToRgbColor(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    value = value.Trim().TrimStart('#');

                    if (value.Length == 6)
                    {
                        var r = Convert.ToInt32(value.Substring(0, 2), 16);
                        var g = Convert.ToInt32(value.Substring(2, 2), 16);
                        var b = Convert.ToInt32(value.Substring(4, 2), 16);

                        return $"{r}, {g}, {b}";
                    }
                }
                catch { }
            }

            return "0, 0, 0";
        }
    }
}