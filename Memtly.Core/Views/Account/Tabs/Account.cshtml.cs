using Microsoft.AspNetCore.Mvc.RazorPages;
using Memtly.Core.Models.Database;

namespace Memtly.Core.Views.Account.Tabs
{
    public class AccountModel : PageModel
    {
        public AccountModel() 
        {
        }
            
        public UserModel? Account { get; set; }

        public void OnGet()
        {
        }
    }
}