using Microsoft.AspNetCore.Mvc.RazorPages;
using Memtly.Core.Models;

namespace Memtly.Core.Views.MediaViewer
{
    public class Popup : PageModel
    {
        public void OnGet()
        {
        }

        public int Id { get; set; } = 0;
        public string? Collection { get; set; }
        public string? Source { get; set; }
        public string? Thumbnail { get; set; }
        public string? Author { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; } = "Image";
        public bool DownloadEnabled { get; set; } = false;
        public PhotoGalleryImageLikes? Likes { get; set; } = null;
    }
}