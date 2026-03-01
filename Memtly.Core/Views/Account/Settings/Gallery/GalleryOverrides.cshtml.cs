using Microsoft.AspNetCore.Mvc.RazorPages;
using Memtly.Core.Models.Database;

namespace Memtly.Core.Views.Account.Settings.Gallery
{
    public class GalleryOverridesModel : PageModel
    {
        public GalleryOverridesModel()
        {
        }

        public IDictionary<string, string>? Settings { get; set; }
        
        public IEnumerable<CustomResourceModel>? CustomResources { get; set; }

        public void OnGet()
        {
        }
    }
}