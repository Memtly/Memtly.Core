using Memtly.Core.Enums;

namespace Memtly.Core.Models
{
    public class Permissions
    {
        public Permissions()
        {
            Account = AccountPermissions.None;
            Review = ReviewPermissions.None;
            Collection = CollectionPermissions.None;
            Gallery = GalleryPermissions.None;
            Users = UserPermissions.None;
            CustomResources = CustomResourcePermissions.None;
            Settings = SettingsPermissions.None;
            Audit = AuditPermissions.None;
            Data = DataPermissions.None;
            BackgroundWorkerPermissions = BackgroundWorkerPermissions.None;
            Features = FeaturePermissions.None;
        }

        public AccountPermissions Account { get; set; }
        public ReviewPermissions Review { get; set; }
        public CollectionPermissions Collection { get; set; }
        public GalleryPermissions Gallery { get; set; }
        public UserPermissions Users { get; set; }
        public CustomResourcePermissions CustomResources { get; set; }
        public SettingsPermissions Settings { get; set; }
        public AuditPermissions Audit { get; set; }
        public DataPermissions Data { get; set; }
        public BackgroundWorkerPermissions BackgroundWorkerPermissions { get; set; }
        public FeaturePermissions Features { get; set; }
    }

    public class BasicUserPermissions : Permissions
    {
        public BasicUserPermissions()
            : base()
        {
            Account =
                MemtlyCore.Version == MemtlyVersion.Enterprise ? (AccountPermissions.View
                | AccountPermissions.Payments) : AccountPermissions.None;
            Features = 
                FeaturePermissions.UpgradeToUnlock;
            Collection =
                CollectionPermissions.View
                | CollectionPermissions.Update;
            Gallery =
                GalleryPermissions.View
                | GalleryPermissions.Update
                | GalleryPermissions.Share;
            Users =
                UserPermissions.Login
                | UserPermissions.View
                | UserPermissions.Update
                | UserPermissions.Change_Password
                | UserPermissions.Reset_MFA;
            Audit =
                AuditPermissions.View;
        }
    }

    public class PaidUserPermissions : Permissions
    {
        public PaidUserPermissions()
            : base()
        {
            Account =
                MemtlyCore.Version == MemtlyVersion.Enterprise ? (AccountPermissions.View
                | AccountPermissions.Payments) : AccountPermissions.None;
            Review =
                ReviewPermissions.View
                | ReviewPermissions.Approve
                | ReviewPermissions.Reject;
            Collection =
                CollectionPermissions.View
                | CollectionPermissions.Create
                | CollectionPermissions.Update
                | CollectionPermissions.Delete
                | CollectionPermissions.Download;
            Gallery =
                GalleryPermissions.View
                | GalleryPermissions.Create
                | GalleryPermissions.Update
                | GalleryPermissions.Delete
                | GalleryPermissions.Upload
                | GalleryPermissions.Download
                | GalleryPermissions.Share
                | GalleryPermissions.Wipe;
            Users =
                UserPermissions.Login
                | UserPermissions.View
                | UserPermissions.Update
                | UserPermissions.Change_Password
                | UserPermissions.Reset_MFA;
            CustomResources =
                CustomResourcePermissions.View
                | CustomResourcePermissions.Create
                | CustomResourcePermissions.Update
                | CustomResourcePermissions.Delete;
            Settings =
                SettingsPermissions.Collection_Update
                | SettingsPermissions.Gallery_Update;
            Audit =
                AuditPermissions.View;
            Features = 
                FeaturePermissions.RequireGalleryItemReview;
        }
    }

    public class ReviewerPermissions : Permissions
    {
        public ReviewerPermissions()
            : base()
        {
            Account =
                MemtlyCore.Version == MemtlyVersion.Enterprise ? AccountPermissions.View : AccountPermissions.None;
            Review =
                ReviewPermissions.View
                | ReviewPermissions.Approve
                | ReviewPermissions.Reject
                | ReviewPermissions.Delete;
            Collection =
                CollectionPermissions.View;
            Gallery =
                GalleryPermissions.View;
            Users =
                UserPermissions.Login;
        }
    }

    public class ModeratorPermissions : Permissions
    {
        public ModeratorPermissions()
            : base()
        {
            Account =
                MemtlyCore.Version == MemtlyVersion.Enterprise ? AccountPermissions.View : AccountPermissions.None;
            Review =
                ReviewPermissions.View
                | ReviewPermissions.Approve
                | ReviewPermissions.Reject
                | ReviewPermissions.Delete;
            Collection =
                CollectionPermissions.View
                | CollectionPermissions.Update
                | CollectionPermissions.Download;
            Gallery =
                GalleryPermissions.View
                | GalleryPermissions.Update
                | GalleryPermissions.Upload
                | GalleryPermissions.Share
                | GalleryPermissions.Download;
            Users =
                UserPermissions.Login
                | UserPermissions.View
                | UserPermissions.Reset_MFA
                | UserPermissions.Freeze;
            CustomResources =
                CustomResourcePermissions.View;
            Audit =
                AuditPermissions.View;
            Features =
                FeaturePermissions.RequireGalleryItemReview
                | FeaturePermissions.RetainRejectedItems;
        }
    }

    public class AdminPermissions : Permissions
    {
        public AdminPermissions()
            : base()
        {
            Account =
                MemtlyCore.Version == MemtlyVersion.Enterprise ? AccountPermissions.View : AccountPermissions.None;
            Review =
                 ReviewPermissions.View
                 | ReviewPermissions.Approve
                 | ReviewPermissions.Reject
                 | ReviewPermissions.Delete;
            Collection =
                CollectionPermissions.View
                | CollectionPermissions.Create
                | CollectionPermissions.Update
                | CollectionPermissions.Delete
                | CollectionPermissions.Download;
            Gallery =
                GalleryPermissions.View
                | GalleryPermissions.ViewAllGallery
                | GalleryPermissions.Create
                | GalleryPermissions.Update
                | GalleryPermissions.Delete
                | GalleryPermissions.Upload
                | GalleryPermissions.Download
                | GalleryPermissions.Wipe
                | GalleryPermissions.Relink
                | GalleryPermissions.Share;
            Users =
                UserPermissions.Login
                | UserPermissions.View
                | UserPermissions.Create
                | UserPermissions.Update
                | UserPermissions.Delete
                | UserPermissions.Change_Password
                | UserPermissions.Change_Permissions_Level
                | UserPermissions.Reset_MFA
                | UserPermissions.Freeze;
            CustomResources =
                CustomResourcePermissions.View
                | CustomResourcePermissions.Create
                | CustomResourcePermissions.Update
                | CustomResourcePermissions.Delete
                | CustomResourcePermissions.Relink;
            Settings =
                SettingsPermissions.View
                | SettingsPermissions.Update
                | SettingsPermissions.Collection_Update
                | SettingsPermissions.Gallery_Update;
            Audit =
                AuditPermissions.View;
            Data =
                DataPermissions.View
                | DataPermissions.Import
                | DataPermissions.Export
                | DataPermissions.Wipe;
            BackgroundWorkerPermissions =
                BackgroundWorkerPermissions.RequestInstantRun
                | BackgroundWorkerPermissions.RequestDirectoryScanner
                | BackgroundWorkerPermissions.RequestCleanup;
            Features =
                FeaturePermissions.RequireGalleryItemReview
                | FeaturePermissions.RetainRejectedItems;
        }
    }
}