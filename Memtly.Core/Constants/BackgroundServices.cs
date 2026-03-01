namespace Memtly.Core.Constants
{
    public class BackgroundServices
    {
        public class DirectoryScanner
        {
            public const string BaseKey = "BackgroundServices:Directory_Scanner:";
            public const string Enabled = "BackgroundServices:Directory_Scanner:Enabled";
            public const string Schedule = "BackgroundServices:Directory_Scanner:Schedule";
        }

        public class EmailReport
        {
            public const string BaseKey = "BackgroundServices:Email_Report:";
            public const string Enabled = "BackgroundServices:Email_Report:Enabled";
            public const string Schedule = "BackgroundServices:Email_Report:Schedule";
        }

        public class Cleanup
        {
            public const string BaseKey = "BackgroundServices:Cleanup:";
            public const string Enabled = "BackgroundServices:Cleanup:Enabled";
            public const string Schedule = "BackgroundServices:Cleanup:Schedule";
        }
    }
}