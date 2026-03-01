using System.Text.RegularExpressions;
using System.Web;

namespace Memtly.Core.Helpers
{
    public class HtmlSanitizer
    {
        public static string Sanitize(string input)
        {
            var output = input;

            if (!string.IsNullOrWhiteSpace(output))
            {
                output = SanitizeHtmlTags(input, new[] { ".*" });
                output = SanitizeHtmlAttributes(output, new[] { ".*" });

                output = SanitizeLinks(output);

                output = HttpUtility.HtmlEncode(output);
            }

            return output;
        }

        public static string SanitizeHtmlTags(string input, string[] tags)
        {
            var output = input;

            foreach (var tag in tags)
            {
                output = new Regex($"<\\/?\\s*{tag}\\s*[^>]*>", RegexOptions.IgnoreCase).Replace(output, string.Empty);
            }

            return output;
        }

        public static string SanitizeHtmlAttributes(string input, string[] attrs)
        {
            var output = input;

            foreach (var attr in attrs)
            {
                output = new Regex($"{attr}\\s*=\\s*['\"].*?['\"]", RegexOptions.IgnoreCase).Replace(output, string.Empty);
            }

            return output;
        }

        public static string SanitizeLinks(string input)
        {
            var output = input;

            output = new Regex(@"href\s*=\s*['""]javascript:[^'""]*['""]", RegexOptions.IgnoreCase).Replace(output, string.Empty);
            output = new Regex(@"(http|https):\/\/[^\s<>]+", RegexOptions.IgnoreCase).Replace(output, string.Empty);

            return output;
        }

        public static bool MayContainXss(string input)
        {
            var sanitized = Sanitize(input);

            return !string.Equals(input, sanitized);
        }
    }
}