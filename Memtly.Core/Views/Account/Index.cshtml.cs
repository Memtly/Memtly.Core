using Microsoft.AspNetCore.Mvc.RazorPages;
using Memtly.Core.Enums;
using Memtly.Core.Models;
using Memtly.Core.Models.Database;

namespace Memtly.Core.Views.Account
{
    public class IndexModel : PageModel
    {
        public IndexModel() 
        {
        }

        public AccountTabs ActiveTab { get; set; } = AccountTabs.Account;
        public UserModel? Account { get; set; }
        public List<PhotoGallery>? PendingRequests { get; set; }
        public List<UserModel>? Users { get; set; }
        public List<GalleryModel>? Galleries { get; set; }
        public List<CustomResourceModel>? CustomResources { get; set; }
        public IEnumerable<AuditLogModel>? AuditLogs { get; set; }
        public IDictionary<string, string>? Settings { get; set; }

        public int TotalItems { get; set; } = 0;
        public int TotalItemsPerPage { get; set; } = 50;

        public void OnGet()
        {
        }
    }
}