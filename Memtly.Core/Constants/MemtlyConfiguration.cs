namespace Memtly.Core.Constants
{
    public class MemtlyConfiguration
    {
        public const string IsDemoMode = "Memtly:Demo_Mode";

        public class Database
        {
            public const string BaseKey = "Memtly:Database:";
            public const string Type = "Memtly:Database:Type";
            public const string ConnectionString = "Memtly:Database:Connection_String";
            public const string SyncFromConfig = "Memtly:Database:Sync_From_Config";
        }

        public class Security
        {
            public class Encryption
            {
                public const string Key = "Memtly:Security:Encryption:Key";
                public const string Salt = "Memtly:Security:Encryption:Salt";
                public const string Iterations = "Memtly:Security:Encryption:Iterations";
                public const string HashType = "Memtly:Security:Encryption:HashType";
            }

            public class Headers
            {
                public const string Enabled = "Memtly:Security:Headers:Enabled";
                public const string XFrameOptions = "Memtly:Security:Headers:X_Frame_Options";
                public const string XContentTypeOptions = "Memtly:Security:Headers:X_Content_Type_Options";
                public const string CSP = "Memtly:Security:Headers:CSP";
            }

            public class MultiFactor
            {
                public const string ResetToDefault = "Memtly:Security:MultiFactor:Reset_To_Default";
            }

            public class Hardening
            { 
                public const string AllowInsecureGalleries = "Memtly:Security:Hardening:Allow_Insecure_Galleries";
            }
        }

        public class Basic
        {
            public const string BaseKey = "Memtly:";
            public const string Title = "Memtly:Title";
            public const string Logo = "Memtly:Logo";
            public const string BaseUrl = "Memtly:Base_Url";
            public const string DefaultGallerySecretKey = "Memtly:Gallery_Secret_Key";
            public const string ForceHttps = "Memtly:Force_Https";
            public const string SingleGalleryMode = "Memtly:Single_Gallery_Mode";
            public const string MaxGalleryCount = "Memtly:Max_Gallery_Count";
            public const string HomeLink = "Memtly:Home_Link";
            public const string GuestGalleryCreation = "Memtly:Guest_Gallery_Creation";
            public const string HideKeyFromQRCode = "Memtly:Hide_Key_From_QR_Code";
            public const string LinksOpenNewTab = "Memtly:Links_Open_New_Tab";
            public const string ThumbnailSize = "Memtly:Thumbnail_Size";
        }

        public class Account
        {
            public const string BaseKey = "Memtly:Account:";
            public const string ShowProfileIcon = "Memtly:Account:Show_Profile_Icon";
            public const string LockoutAttempts = "Memtly:Account:Lockout_Attempts";
            public const string LockoutMins = "Memtly:Account:Lockout_Mins";

            public class Admin
            {
                public const string BaseKey = "Memtly:Account:Admin:";
                public const string EmailAddress = "Memtly:Account:Admin:Email";
                public const string Password = "Memtly:Account:Admin:Password";
            }

            public class Registration
            {
                public const string BaseKey = "Memtly:Account:Registration:";
                public const string Enabled = "Memtly:Account:Registration:Enabled";
                public const string RequireEmailValidation = "Memtly:Account:Registration:Require_Email_Validation";
            }
        }

        public class GallerySelector
        {
            public const string BaseKey = "Memtly:Gallery_Selector:";
            public const string Dropdown = "Memtly:Gallery_Selector:Dropdown";
            public const string HideDefaultOption = "Memtly:Gallery_Selector:Hide_Default_Option";
            public const string ShowGalleryIdentifiers = "Memtly:Gallery_Selector:Show_Gallery_Identifiers";
            public const string ShowGalleryNames = "Memtly:Gallery_Selector:Show_Gallery_Names";
            public const string ShowUsernames = "Memtly:Gallery_Selector:Show_Usernames";
        }

        public class Gallery
        {
            public const string BaseKey = "Memtly:Gallery:";
            public const string ShowTitle = "Memtly:Gallery:Show_Title";
            public const string BannerImage = "Memtly:Gallery:Banner_Image";
            public const string Quote = "Memtly:Gallery:Quote";
            public const string Columns = "Memtly:Gallery:Columns";
            public const string ItemsPerPage = "Memtly:Gallery:Items_Per_Page";
            public const string FullWidth = "Memtly:Gallery:Full_Width";
            public const string RetainRejectedItems = "Memtly:Gallery:Retain_Rejected_Items";
            public const string Likes = "Memtly:Gallery:Likes";
            public const string Upload = "Memtly:Gallery:Upload";
            public const string Download = "Memtly:Gallery:Download";
            public const string RequireReview = "Memtly:Gallery:Require_Review";
            public const string ReviewCounter = "Memtly:Gallery:Review_Counter";
            public const string PreventDuplicates = "Memtly:Gallery:Prevent_Duplicates";
            public const string IdleRefreshMins = "Memtly:Gallery:Idle_Refresh_Mins";
            public const string MaxSizeMB = "Memtly:Gallery:Max_Size_MB";
            public const string MaxFileSizeMB = "Memtly:Gallery:Max_File_Size_MB";
            public const string DefaultView = "Memtly:Gallery:Default_View";
            public const string DefaultGroup = "Memtly:Gallery:Default_Group";
            public const string DefaultOrder = "Memtly:Gallery:Default_Order";
            public const string DefaultFilter = "Memtly:Gallery:Default_Filter";
            public const string UploadPeriod = "Memtly:Gallery:Upload_Period";
            public const string AllowedFileTypes = "Memtly:Gallery:Allowed_File_Types";
            public const string CameraUploads = "Memtly:Gallery:Camera_Uploads";
            public const string ShowFilters = "Memtly:Gallery:Show_Filters";

            public class QRCode
            {
                public const string BaseKey = "Memtly:Gallery:QR_Code:";
                public const string Enabled = "Memtly:Gallery:QR_Code:Enabled";
                public const string DefaultView = "Memtly:Gallery:QR_Code:Default_View";
                public const string DefaultSort = "Memtly:Gallery:QR_Code:Default_Sort";
                public const string IncludeCulture = "Memtly:Gallery:QR_Code:Include_Culture";
            }
        }

        public class Slideshow
        {
            public const string BaseKey = "Memtly:Slideshow:";
            public const string Interval = "Memtly:Slideshow:Interval";
            public const string Fade = "Memtly:Slideshow:Fade";
            public const string Limit = "Memtly:Slideshow:Limit";
            public const string IncludeShareSlide = "Memtly:Slideshow:Include_Share_Slide";
        }
        
        public class Alerts
        {
            public const string BaseKey = "Memtly:Alerts:";
            public const string FailedLogin = "Memtly:Alerts:Failed_Login";
            public const string AccountLockout = "Memtly:Alerts:Account_Lockout";
            public const string DestructiveAction = "Memtly:Alerts:Destructive_Action";
            public const string PendingReview = "Memtly:Alerts:Pending_Review";
        }

        public class Reports
        {
            public const string BaseKey = "Memtly:Reports:";

            public class Email
            {
                public const string Enabled = "Memtly:Reports:Email:Enabled";
                public const string Schedule = "Memtly:Reports:Email:Schedule";
            }
        }

        public class Notifications
        {

            public class Gotify
            {
                public const string Enabled = "Memtly:Notifications:Gotify:Enabled";
                public const string Endpoint = "Memtly:Notifications:Gotify:Endpoint";
                public const string Token = "Memtly:Notifications:Gotify:Token";
                public const string Priority = "Memtly:Notifications:Gotify:Priority";
            }

            public class Ntfy
            {
                public const string Enabled = "Memtly:Notifications:Ntfy:Enabled";
                public const string Endpoint = "Memtly:Notifications:Ntfy:Endpoint";
                public const string Token = "Memtly:Notifications:Ntfy:Token";
                public const string Topic = "Memtly:Notifications:Ntfy:Topic";
                public const string Priority = "Memtly:Notifications:Ntfy:Priority";
                public const string Icon = "Memtly:Notifications:Ntfy:Icon";
            }

            public class Smtp
            {
                public const string Enabled = "Memtly:Notifications:Smtp:Enabled";
                public const string Recipient = "Memtly:Notifications:Smtp:Recipient";
                public const string Host = "Memtly:Notifications:Smtp:Host";
                public const string Port = "Memtly:Notifications:Smtp:Port";
                public const string Username = "Memtly:Notifications:Smtp:Username";
                public const string Password = "Memtly:Notifications:Smtp:Password";
                public const string From = "Memtly:Notifications:Smtp:From";
                public const string DisplayName = "Memtly:Notifications:Smtp:DisplayName";
                public const string UseSSL = "Memtly:Notifications:Smtp:Use_SSL";
            }
        }

        public class Audit
        {
            public const string BaseKey = "Memtly:Audit:";
            public const string Enabled = "Memtly:Audit:Enabled";
            public const string Retention = "Memtly:Audit:Retention";
        }

        public class IdentityCheck
        {
            public const string BaseKey = "Memtly:Identity_Check:";
            public const string Enabled = "Memtly:Identity_Check:Enabled";
            public const string ShowOnPageLoad = "Memtly:Identity_Check:Show_On_Page_Load";
            public const string RequireIdentityForUpload = "Memtly:Identity_Check:Require_Identity_For_Upload";
            public const string RequireName = "Memtly:Identity_Check:Require_Name";
            public const string RequireEmail = "Memtly:Identity_Check:Require_Email";
        }

        public class Policies
        {
            public const string BaseKey = "Memtly:Policies:";
            public const string Enabled = "Memtly:Policies:Enabled";
            public const string CookiePolicy = "Memtly:Policies:CookiePolicy";
        }

        public class Trackers
        {
            public const string BaseKey = "Memtly:Trackers:";

            public class Umami
            {
                public const string BaseKey = "Memtly:Trackers:Umami:";
                public const string Endpoint = "Memtly:Trackers:Umami:Endpoint";
                public const string ScriptName = "Memtly:Trackers:Umami:ScriptName";
                public const string WebsiteId = "Memtly:Trackers:Umami:WebsiteId";

                public class PerformanceTracking
                {
                    public const string BaseKey = "Memtly:Trackers:Umami:PerformanceTracking:";
                    public const string Enabled = "Memtly:Trackers:Umami:PerformanceTracking:Enabled";
                }

                public class Replay
                {
                    public const string BaseKey = "Memtly:Trackers:Umami:Replay:";
                    public const string Enabled = "Memtly:Trackers:Umami:Replay:Enabled";
                    public const string SampleRate = "Memtly:Trackers:Umami:Replay:Sample_Rate";
                    public const string MaskLevel = "Memtly:Trackers:Umami:Replay:Mask_Level";
                    public const string MaxDuration = "Memtly:Trackers:Umami:Replay:Max_Duration";
                    public const string BlockSelector = "Memtly:Trackers:Umami:Replay:Block_Selector";
                }
            }
        }

        public class Themes
        {
            public const string BaseKey = "Memtly:Themes:";
            public const string Enabled = "Memtly:Themes:Enabled";
            public const string Default = "Memtly:Themes:Default";
        }

        public class Languages
        {
            public const string BaseKey = "Memtly:Languages:";
            public const string Enabled = "Memtly:Languages:Enabled";
            public const string Default = "Memtly:Languages:Default";
        }

        public class Sponsors
        {
            public const string Url = "Memtly:Sponsors:Url";
            public const string Endpoint = "Memtly:Sponsors:Endpoint";

            public class Github
            {
                public const string ProfileUrl = "Memtly:Sponsors:Github:ProfileUrl";
            }
        }

        public class BackgroundServices
        {
            public class DirectoryScanner
            {
                public const string BaseKey = "Memtly:BackgroundServices:Directory_Scanner:";
                public const string Enabled = "Memtly:BackgroundServices:Directory_Scanner:Enabled";
                public const string Schedule = "Memtly:BackgroundServices:Directory_Scanner:Schedule";
            }

            public class Cleanup
            {
                public const string BaseKey = "Memtly:BackgroundServices:Cleanup:";
                public const string Enabled = "Memtly:BackgroundServices:Cleanup:Enabled";
                public const string Schedule = "Memtly:BackgroundServices:Cleanup:Schedule";
            }
        }
    }

    public class MemtlyStaticConfiguration
    {
        public class Links
        {
            public const string GitHub = "Links:GitHub";
            public const string GitHubSponsors = "Links:GitHubSponsors";
            public const string GitHubStargazers = "Links:GitHubStargazers";
            public const string BuyMeACoffee = "Links:BuyMeACoffee";
        }
    }
}