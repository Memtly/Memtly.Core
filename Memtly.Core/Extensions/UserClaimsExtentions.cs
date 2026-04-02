using System.Security.Claims;
using System.Security.Principal;
using Memtly.Core;
using Memtly.Core.Enums;
using Memtly.Core.Models;

namespace Memtly.Core.Extensions
{
    public static class UserClaimsExtentions
    {
        public static int GetUserId(this IIdentity identity)
        {
            try
            {
                return int.Parse(((ClaimsIdentity)identity).Claims.FirstOrDefault(x => string.Equals(ClaimTypes.Sid, x.Type, StringComparison.OrdinalIgnoreCase))?.Value ?? "-1");
            }
            catch { }

            return -1;
        }

        public static UserLevel GetUserLevel(this IIdentity identity)
        {
            try
            {
                var role = ((ClaimsIdentity)identity).Claims.FirstOrDefault(x => string.Equals(ClaimTypes.Role, x.Type, StringComparison.OrdinalIgnoreCase))?.Value ?? $"{UserLevel.Basic}|{PaidTier.None}";
                var level = role.Split(new [] { '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];

                foreach (UserLevel l in Enum.GetValues(typeof(UserLevel)))
                {
                    if (l.ToString().Equals(level, StringComparison.OrdinalIgnoreCase))
                    {
                        return l;
                    }
                }
            }
            catch { }

            return UserLevel.Basic;
        }

        public static PaidTier GetPaidTier(this IIdentity identity)
        {
            if (MemtlyCore.Version == MemtlyVersion.Enterprise)
            { 
                try
                {
                    var role = ((ClaimsIdentity)identity).Claims.FirstOrDefault(x => string.Equals(ClaimTypes.Role, x.Type, StringComparison.OrdinalIgnoreCase))?.Value ?? $"{UserLevel.Basic}|{PaidTier.None}";
                    var tier = role.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[1];

                    foreach (PaidTier t in Enum.GetValues(typeof(PaidTier)))
                    {
                        if (t.ToString().Equals(tier, StringComparison.OrdinalIgnoreCase))
                        {
                            return t;
                        }
                    }
                }
                catch { }
            }

            return PaidTier.None;
        }

        public static bool IsPrivilegedUser(this IIdentity identity)
        {
            try
            {
                var userLevel = identity?.GetUserLevel() ?? UserLevel.Basic;
                
                return userLevel == UserLevel.Reviewer
                    || userLevel == UserLevel.Moderator
                    || userLevel == UserLevel.Admin;
            }
            catch { }

            return false;
        }

        public static bool IsBasicUser(this IIdentity identity)
        {
            return identity == null || identity.IsPrivilegedUser() == false;
        }

        public static Permissions GetUserPermissions(this IIdentity identity)
        {
            try
            {
                var level = identity.GetUserLevel();
                switch (level)
                {
                    case UserLevel.Basic:
                        return new BasicUserPermissions();
                    case UserLevel.Paid:
                        return MemtlyCore.Version == MemtlyVersion.Enterprise ? new PaidUserPermissions() : new BasicUserPermissions();
                    case UserLevel.Reviewer:
                        return new ReviewerPermissions();
                    case UserLevel.Moderator:
                        return new ModeratorPermissions();
                    case UserLevel.Admin:
                        return new AdminPermissions();
                    default: 
                        return new Permissions();
                }
            }
            catch { }

            return new Permissions();
        }

        public static int GetGalleryLimit(this IIdentity identity)
        {
            if (MemtlyCore.Version == MemtlyVersion.Enterprise)
            {
                try
                {
                    switch (identity.GetUserLevel())
                    {
                        case UserLevel.Basic:
                            return 1;
                        case UserLevel.Paid:
                            switch (identity.GetPaidTier())
                            {
                                case PaidTier.Basic:
                                    return 3;
                                case PaidTier.Advanced:
                                    return 5;
                                case PaidTier.Premium:
                                    return 10;
                                default:
                                    return 1;
                            }
                        case UserLevel.Reviewer:
                            return 0;
                        case UserLevel.Moderator:
                            return 3;
                        case UserLevel.Admin:
                            return int.MaxValue;
                    }
                }
                catch { }

                return 0;
            }

            return int.MaxValue;
        }

        public static AccountTabs GetDefaultTab(this IIdentity identity)
        {
            var userPermissions = identity?.GetUserPermissions() ?? new Permissions();
            if (userPermissions.Account.HasFlag(AccountPermissions.View) && MemtlyCore.Version == MemtlyVersion.Enterprise)
            {
                return AccountTabs.Account;
            }
            else if (userPermissions.Review.HasFlag(ReviewPermissions.View))
            {
                return AccountTabs.Reviews;
            }
            else if (userPermissions.Gallery.HasFlag(GalleryPermissions.View))
            {
                return AccountTabs.Galleries;
            }
            else if (userPermissions.Users.HasFlag(UserPermissions.View))
            {
                return AccountTabs.Users;
            }
            else if (userPermissions.CustomResources.HasFlag(CustomResourcePermissions.View))
            {
                return AccountTabs.Resources;
            }
            else if (userPermissions.Settings.HasFlag(SettingsPermissions.View))
            {
                return AccountTabs.Settings;
            }
            else if (userPermissions.Audit.HasFlag(AuditPermissions.View))
            {
                return AccountTabs.Audit;
            }

            return AccountTabs.Reviews;
        }

        public static bool CanEdit(this IIdentity identity, Enum type, int? ownerId)
        {
            try
            {
                if (identity != null)
                {
                    var level = identity.GetUserLevel();
                    var permissions = identity.GetUserPermissions();

                    if (identity.IsPrivilegedUser())
                    {
                        return true;
                    }
                    else
                    {
                        var hasPermissions = false;
                        if (type.GetType() == typeof(AccountPermissions))
                        {
                            hasPermissions = permissions.Account.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(ReviewPermissions))
                        {
                            hasPermissions = permissions.Review.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(GalleryPermissions))
                        {
                            hasPermissions = permissions.Gallery.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(UserPermissions))
                        {
                            hasPermissions = permissions.Users.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(CustomResourcePermissions))
                        {
                            hasPermissions = permissions.CustomResources.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(SettingsPermissions))
                        {
                            hasPermissions = permissions.Settings.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(AuditPermissions))
                        {
                            hasPermissions = permissions.Audit.HasFlag(type);
                        }
                        else if (type.GetType() == typeof(DataPermissions))
                        {
                            hasPermissions = permissions.Data.HasFlag(type);
                        }

                        return hasPermissions && (ownerId != null && identity.GetUserId() == ownerId);
                    }
                }
            }
            catch { }

            return false;
        }
    }
}