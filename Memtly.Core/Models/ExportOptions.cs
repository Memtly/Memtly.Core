namespace Memtly.Core.Models
{
    public class ExportOptions
    {
        public bool Database { get; set; } = true;
        public bool Uploads { get; set; } = true;
        public bool Thumbnails { get; set; } = true;
        public bool CustomResources { get; set; } = true;
    }
}