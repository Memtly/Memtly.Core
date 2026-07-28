using System.Text.Json.Serialization;

namespace Memtly.Core.Models
{
    public class ShareSelectItem
    {
        public ShareSelectItem(int id, string name, bool selected = false)
        {
            this.Id = id;
            this.Name = name;
            this.Selected = selected;
        }

        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("selected")]
        public bool Selected { get; set; } = false;
    }
}