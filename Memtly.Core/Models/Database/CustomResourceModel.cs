namespace Memtly.Core.Models.Database
{
    public class CustomResourceModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int Owner { get; set; }
        public string OwnerName { get; set; } = "Unknown";
    }
}