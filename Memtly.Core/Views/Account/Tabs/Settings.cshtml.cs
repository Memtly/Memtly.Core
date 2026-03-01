using Microsoft.AspNetCore.Mvc.RazorPages;
using Memtly.Core.Models.Database;

namespace Memtly.Core.Views.Account.Tabs
{
    public class SettingsModel : PageModel
    {
        public SettingsModel() 
        {
        }

        public IDictionary<string, string>? Settings { get; set; }
        
        public IEnumerable<CustomResourceModel>? CustomResources { get; set; }

        public void OnGet()
        {
        }
    }
}