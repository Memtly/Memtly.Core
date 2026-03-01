namespace Memtly.Core.Constants
{
    public class Notifications
    {
        public class Alerts
        {
            public const string FailedLogin = "Notifications:Alerts:Failed_Login";
            public const string AccountLockout = "Notifications:Alerts:Account_Lockout";
            public const string DestructiveAction = "Notifications:Alerts:Destructive_Action";
            public const string PendingReview = "Notifications:Alerts:Pending_Review";
        }

        public class Gotify
        {
            public const string Enabled = "Notifications:Gotify:Enabled";
            public const string Endpoint = "Notifications:Gotify:Endpoint";
            public const string Token = "Notifications:Gotify:Token";
            public const string Priority = "Notifications:Gotify:Priority";
        }

        public class Ntfy
        {
            public const string Enabled = "Notifications:Ntfy:Enabled";
            public const string Endpoint = "Notifications:Ntfy:Endpoint";
            public const string Token = "Notifications:Ntfy:Token";
            public const string Topic = "Notifications:Ntfy:Topic";
            public const string Priority = "Notifications:Ntfy:Priority";
        }

        public class Smtp
        {
            public const string Enabled = "Notifications:Smtp:Enabled";
            public const string Recipient = "Notifications:Smtp:Recipient";
            public const string Host = "Notifications:Smtp:Host";
            public const string Port = "Notifications:Smtp:Port";
            public const string Username = "Notifications:Smtp:Username";
            public const string Password = "Notifications:Smtp:Password";
            public const string From = "Notifications:Smtp:From";
            public const string DisplayName = "Notifications:Smtp:DisplayName";
            public const string UseSSL = "Notifications:Smtp:Use_SSL";
        }
    }
}