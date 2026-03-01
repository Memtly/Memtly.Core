using System.Text.Json.Serialization;

namespace Memtly.Core.Models
{
    public class SponsorsList
    {
        [JsonPropertyName("tiers")]
        public IEnumerable<SponsorsTier>? Tiers { get; set; }
    }

    public class SponsorsTier
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("platforms")]
        public IEnumerable<SponsorPlatform>? Platforms { get; set; }
    }

    public class SponsorPlatform
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("sponsors")]
        public IEnumerable<string>? Sponsors { get; set; }
    }
}