namespace Memtly.Core.Constants
{
    public static class SessionKey
    {
        public static class Device
        {
            public const string Type = "DeviceType";
        }

        public static class Viewer
        {
            public const string Identity = "ViewerIdentity";
            public const string EmailAddress = "ViewerEmailAddress";
        }

        public static class MultiFactor
        {
            public const string TokenSet = "2FA_SET";
            public const string Secret = "2FA_SECRET";
            public const string QR = "2FA_QR_CODE";
        }

        public static class Language
        {
            public const string Selected = "SelectedLanguage";
        }

        public static class Theme
        {
            public const string Selected = "Theme";
        }
    }
}