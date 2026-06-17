namespace Memtly.Core.Models
{
    public class GalleryIdentifierModel
    {
        public GalleryIdentifierModel()
            : this(-1, string.Empty, string.Empty)
        {
        }

        public GalleryIdentifierModel(int id, string identifier, string name)
        {
            this.Id = id;
            this.Identifier = identifier;
            this.Name = name;
        }

        public int Id { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}