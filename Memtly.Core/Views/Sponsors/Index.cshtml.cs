using Microsoft.AspNetCore.Mvc.RazorPages;
using Memtly.Core.Models;

namespace Memtly.Core.Views.Sponsors
{
    public class IndexModel : PageModel
    {
        public IndexModel() 
        {
        }

        public SponsorsList? SponsorsList { get; set; }

        public void OnGet()
        {
        }
    }
}