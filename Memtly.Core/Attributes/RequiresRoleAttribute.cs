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
        public GalleryPermissions GalleryPermission { get; set; } = GalleryPermissions.None;
        public UserPermissions UserPermission { get; set; } = UserPermissions.None;
        public CustomResourcePermissions CustomResourcePermission { get; set; } = CustomResourcePermissions.None;
        public SettingsPermissions SettingsPermission { get; set; } = SettingsPermissions.None;
        public AuditPermissions AuditPermission { get; set; } = AuditPermissions.None;
        public DataPermissions DataPermission { get; set; } = DataPermissions.None;

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
                    (pemissions.Review != ReviewPermissions.None && !pemissions.Review.HasFlag(this.ReviewPermission))
                    || (pemissions.Gallery != GalleryPermissions.None && !pemissions.Gallery.HasFlag(this.GalleryPermission))
                    || (pemissions.Users != UserPermissions.None && !pemissions.Users.HasFlag(this.UserPermission))
                    || (pemissions.CustomResources != CustomResourcePermissions.None && !pemissions.CustomResources.HasFlag(this.CustomResourcePermission))
                    || (pemissions.Settings != SettingsPermissions.None && !pemissions.Settings.HasFlag(this.SettingsPermission))
                    || (pemissions.Audit != AuditPermissions.None && !pemissions.Audit.HasFlag(this.AuditPermission))
                    || (pemissions.Data != DataPermissions.None && !pemissions.Data.HasFlag(this.DataPermission))
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