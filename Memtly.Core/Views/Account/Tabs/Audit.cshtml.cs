using Microsoft.AspNetCore.Mvc.RazorPages;
using Memtly.Core.Models.Database;

namespace Memtly.Core.Views.Account.Tabs
{
    public class AuditModel : PageModel
    {
        public AuditModel() 
        {
        }

        public IEnumerable<AuditLogModel>? Logs { get; set; }

        public void OnGet()
        {
        }
    }
}