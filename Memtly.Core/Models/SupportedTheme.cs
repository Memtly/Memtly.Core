using System.Text.Json.Serialization;
using Memtly.Core.Enums;

namespace Memtly.Core.Models
{
    public class SupportedThemeList
    {
        [JsonPropertyName("themes")]
        public List<SupportedThemes> Themes { get; set; } = new List<SupportedThemes>();

        [JsonPropertyName("selected")]
        public SelectedThemeOption Selected 
        {
            get
            {
                try
                {
                    var theme = this.Themes?.FirstOrDefault(theme => theme.Selected);
                    if (!string.IsNullOrWhiteSpace(theme?.Name) && !string.IsNullOrWhiteSpace(theme?.Value))
                    {
                        return new SelectedThemeOption(theme.Name, theme.Value);
                    }
                }
                catch { }

                return new SelectedThemeOption();
            }
        }
    }

    public class SupportedThemes
    {
        public SupportedThemes(string name = "Auto Detect", string value = "AutoDetect")
        {
            this.Name = name;
            this.Value = value;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("selected")]
        public bool Selected { get; set; } = false;
    }

    public class SelectedThemeOption
    {
        public SelectedThemeOption()
            : this(Themes.AutoDetect.ToString(), Themes.AutoDetect.ToString())
        {
        }

        public SelectedThemeOption(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}