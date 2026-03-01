using Microsoft.AspNetCore.Mvc.RazorPages;
using Memtly.Core.Models.Database;

namespace Memtly.Core.Views.Account.Partials
{
    public class SettingsListModel : PageModel
    {
        public SettingsListModel() 
        {
        }

        public IDictionary<string, string>? Settings { get; set; }
        
        public IEnumerable<CustomResourceModel>? CustomResources { get; set; }

        public void OnGet()
        {
        }
    }
}