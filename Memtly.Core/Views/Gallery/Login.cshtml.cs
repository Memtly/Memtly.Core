using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memtly.Core.Views.Gallery
{
    public class LoginModel : PageModel
    {
        public LoginModel() 
        {
            Identifier = string.Empty;
        }

        public string Identifier { get; set; }

        public void OnGet()
        {
        }
    }
}