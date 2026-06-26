using System.Text.Json;

namespace Memtly.Core.Helpers
{
    public interface IWebpackHelper
    {
        public string GetAssetPath(string name);
    }

    public class WebpackHelper : IWebpackHelper
    {
        private Dictionary<string, string>? _manifest;

        public string GetAssetPath(string name)
        {
            if (_manifest == null)
            {
                var assembly = typeof(WebpackHelper).Assembly;
                using var stream = assembly.GetManifestResourceStream("Memtly.Core.wwwroot.dist.manifest.json")
                    ?? throw new FileNotFoundException("manifest.json not found. Ensure webpack has been run before building.");

                using (var reader = new StreamReader(stream))
                {
                    _manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd())!;
                }
            }

            return _manifest.TryGetValue(name, out var hashed) ? hashed : name;
        }
    }
}