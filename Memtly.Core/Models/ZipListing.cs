namespace Memtly.Core.Models
{
    public class ZipListing
    {
        public ZipListing(string sourcePath, IEnumerable<string> files, string? directory = null, string? filename = null)
        {
            this.SourcePath = sourcePath;
            this.Directory = directory;
            this.FileName = filename;
            this.Files = files;
        }

        public string SourcePath { get; }
        public string? FileName { get; }
        public string? Directory { get; }
        public IEnumerable<string>? Files { get; }
    }

    public class ZipListingScanner
    {
        public ZipListingScanner(string name, string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            this.Name = name;
            this.Path = path;
            this.SearchOption = searchOption;
        }

        public string Name { get; }
        public string Path { get; }
        public SearchOption SearchOption { get; }
    }
}