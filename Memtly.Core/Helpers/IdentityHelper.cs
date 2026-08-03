using System.Security.Claims;
using Memtly.Core.Enums;
using Memtly.Core.Extensions;
using Memtly.Core.Models;

namespace Memtly.Core.Helpers
{
    public interface IIdentityHelper
    {
        int GetUserId(ClaimsPrincipal? user);
        UserLevel GetUserLevel(ClaimsPrincipal? user);
        PaidTier GetPaidTier(ClaimsPrincipal? user);
        bool IsValid(ClaimsPrincipal? user);
        bool IsPrivilegedUser(ClaimsPrincipal? user);
        bool IsBasicUser(ClaimsPrincipal? user);
        bool IsOwner(ClaimsPrincipal? user, int? ownerId);
        Permissions GetUserPermissions(ClaimsPrincipal? user);
        int GetGalleryLimit(ClaimsPrincipal? user);
        AccountTabs GetDefaultTab(ClaimsPrincipal? user);
        bool CanEdit(ClaimsPrincipal? user, Enum type, int? ownerId);
        bool CanUseFeature(ClaimsPrincipal? user, FeaturePermissions feature);
    }

    public class IdentityHelper : IIdentityHelper
    {
        public int GetUserId(ClaimsPrincipal? user)
        {
            return user?.Identity?.GetUserId() ?? -1;
        }

        public UserLevel GetUserLevel(ClaimsPrincipal? user)
        {
            return user?.Identity?.GetUserLevel() ?? UserLevel.Basic;
        }

        public PaidTier GetPaidTier(ClaimsPrincipal? user)
        {
            return user?.Identity?.GetPaidTier() ?? PaidTier.None;
        }
        
        public bool IsValid(ClaimsPrincipal? user)
        {
            return (user?.Identity?.IsAuthenticated ?? false) && GetUserId(user) > 0;
        }

        public bool IsPrivilegedUser(ClaimsPrincipal? user)
        {
            return user?.Identity?.IsPrivilegedUser() ?? false;
        }

        public bool IsBasicUser(ClaimsPrincipal? user)
        {
            return user?.Identity?.IsBasicUser() ?? true;
        }

        public bool IsOwner(ClaimsPrincipal? user, int? ownerId)
        {
            return ownerId != null && IsValid(user) && GetUserId(user) == ownerId;
        }

        public Permissions GetUserPermissions(ClaimsPrincipal? user)
        {
            return user?.Identity?.GetUserPermissions() ?? new Permissions();
        }

        public int GetGalleryLimit(ClaimsPrincipal? user)
        {
            return user?.Identity?.GetGalleryLimit() ?? 1;
        }

        public AccountTabs GetDefaultTab(ClaimsPrincipal? user)
        {
            return user?.Identity?.GetDefaultTab() ?? AccountTabs.Reviews;
        }

        public bool CanEdit(ClaimsPrincipal? user, Enum type, int? ownerId)
        {
            return user?.Identity?.CanEdit(type, ownerId) ?? false;
        }

        public bool CanUseFeature(ClaimsPrincipal? user, FeaturePermissions feature)
        {
            return user?.Identity?.CanUseFeature(feature) ?? false;
        }
    }
}