namespace Memtly.Core.Enums
{
    [Flags]
    public enum AccountPermissions : long
    {
        None = 0,
        View = 1,
        Payments = 2
    }

    [Flags]
    public enum ReviewPermissions : long
    {
        None = 0,
        View = 1,
        Approve = 2,
        Reject = 4,
        Delete = 8,
    }

    [Flags]
    public enum GalleryPermissions : long
    {
        None = 0,
        View = 1,
        Create = 2,
        Update = 4,
        Delete = 8,
        Upload = 16,
        Download = 32,
        Wipe = 64,
        ViewAllGallery = 128,
        Relink = 256
    }

    [Flags]
    public enum UserPermissions : long
    {
        None = 0,
        Login = 1,
        View = 2,
        Create = 4,
        Update = 8,
        Delete = 16,
        Change_Password = 32,
        Change_Permissions_Level = 64,
        Reset_MFA = 128,
        Freeze = 256,
    }

    [Flags]
    public enum CustomResourcePermissions : long
    {
        None = 0,
        View = 1,
        Create = 2,
        Update = 4,
        Delete = 8,
        Relink = 16,
    }

    [Flags]
    public enum SettingsPermissions : long
    {
        None = 0,
        View = 1,
        Update = 2,
        Gallery_Update = 4,
    }

    [Flags]
    public enum AuditPermissions : long
    {
        None = 0,
        View = 1,
    }

    [Flags]
    public enum DataPermissions : long
    {
        None = 0,
        View = 1,
        Import = 2,
        Export = 4,
        Wipe = 8,
    }

    [Flags]
    public enum FeaturePermissions : long
    {
        None = 0,
        UpgradeToUnlock = 1,
    }
}