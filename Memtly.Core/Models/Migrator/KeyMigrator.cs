namespace Memtly.Core.Models.Migrator
{
    public class KeyMigrator
    {
        public KeyMigrator(int priority, string key, Func<string, string>? action = null)
        {
            Priority = priority;
            Key = key;
            MigrationAction = action;
        }

        public string Key { get; set; }
        public Func<string, string>? MigrationAction { get; set; }
        public int Priority { get; set; }
    }
}