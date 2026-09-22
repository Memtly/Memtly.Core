using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Memtly.Core.Constants;
using Memtly.Core.Enums;
using Memtly.Core.Extensions;
using Memtly.Core.Models;

namespace Memtly.Core.Attributes
{
    public class RequiresRoleAttribute : ActionFilterAttribute
    {
        public UserLevel User { get; set; } = UserLevel.Basic;
        public ReviewPermissions ReviewPermission { get; set; } = ReviewPermissions.None;
        public CollectionPermissions CollectionPermission { get; set; } = CollectionPermissions.None;
        public GalleryPermissions GalleryPermission { get; set; } = GalleryPermissions.None;
        public UserPermissions UserPermission { get; set; } = UserPermissions.None;
        public CustomResourcePermissions CustomResourcePermission { get; set; } = CustomResourcePermissions.None;
        public SettingsPermissions SettingsPermission { get; set; } = SettingsPermissions.None;
        public AuditPermissions AuditPermission { get; set; } = AuditPermissions.None;
        public DataPermissions DataPermission { get; set; } = DataPermissions.None;
        public BackgroundWorkerPermissions BackgroundWorkerPermissions { get; set; } = BackgroundWorkerPermissions.None;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var level = filterContext.HttpContext?.User?.Identity?.GetUserLevel() ?? UserLevel.Basic;
                if (level < this.User)
                {
                    filterContext.Result = new RedirectToActionResult("Index", "Error", new { Reason = ErrorCode.Unauthorized }, false);
                }
 
                var pemissions = filterContext.HttpContext?.User?.Identity?.GetUserPermissions() ?? new Permissions();
                if (
                    (this.ReviewPermission != ReviewPermissions.None && !pemissions.Review.HasFlag(this.ReviewPermission))
                    || (this.CollectionPermission != CollectionPermissions.None && !pemissions.Collection.HasFlag(this.CollectionPermission))
                    || (this.GalleryPermission != GalleryPermissions.None && !pemissions.Gallery.HasFlag(this.GalleryPermission))
                    || (this.UserPermission != UserPermissions.None && !pemissions.Users.HasFlag(this.UserPermission))
                    || (this.CustomResourcePermission != CustomResourcePermissions.None && !pemissions.CustomResources.HasFlag(this.CustomResourcePermission))
                    || (this.SettingsPermission != SettingsPermissions.None && !pemissions.Settings.HasFlag(this.SettingsPermission))
                    || (this.AuditPermission != AuditPermissions.None && !pemissions.Audit.HasFlag(this.AuditPermission))
                    || (this.DataPermission != DataPermissions.None && !pemissions.Data.HasFlag(this.DataPermission))
                    || (this.BackgroundWorkerPermissions != BackgroundWorkerPermissions.None && !pemissions.BackgroundWorkerPermissions.HasFlag(this.BackgroundWorkerPermissions))
                )
                {
                    filterContext.Result = new RedirectToActionResult("Index", "Error", new { Reason = ErrorCode.Unauthorized }, false);
                }
            }
            catch (Exception ex)
            {
                var logger = filterContext.HttpContext.RequestServices.GetService<ILogger<RequiresSecretKeyAttribute>>();
                if (logger != null)
                {
                    logger.LogError(ex, $"Failed to validate user role - {ex?.Message}");
                }
            }
        }
    }
}