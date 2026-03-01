using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memtly.Core.Views.Account
{
    public class ResetPasswordViewModel : PageModel
    {
        public ResetPasswordViewModel()
        {
        }

        public string? Data { get; set; }

        public void OnGet()
        {
        }
    }
}