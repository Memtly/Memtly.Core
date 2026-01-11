using System.Reflection;
using System.Text.RegularExpressions;
using DbUp;
using DbUp.Engine;
using WeddingShare.Constants;
using WeddingShare.Enums;
using WeddingShare.Helpers.Database;
using WeddingShare.Models.Database;

namespace WeddingShare.Helpers.Dbup
{
    public sealed class DbupMigrator(IEnvironmentWrapper environment, IConfiguration configuration, IDatabaseHelper database, IFileHelper fileHelper, IEncryptionHelper encryption, ILoggerFactory loggerFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var logger = loggerFactory.CreateLogger<DbupMigrator>();

            fileHelper.CreateDirectoryIfNotExists("config");

            var config = new ConfigHelper(environment, configuration, loggerFactory.CreateLogger<ConfigHelper>());

            var connString = config.GetOrDefault(Settings.Database.ConnectionString, "Data Source=./config/wedding-share.db");
            if (!string.IsNullOrWhiteSpace(connString))
            {
                DatabaseUpgradeResult? dbupResult;

                var dbType = config.GetOrDefault(Settings.Database.Type, "sqlite")?.ToLower();
                switch (dbType)
                {
                    case "sqlite":
                        dbupResult = new DbupSqliteHelper().Migrate(connString);
                        break;
                    case "mysql":
                        var databaseName = config.GetOrDefault(Settings.Database.DatabaseName, "weddingshare");
                        dbupResult = new DbupMySqlHelper().Migrate(connString, databaseName);
                        break;
                    default:
                        var error = $"Database type '{dbType}' is not yet supported by this application";
                        logger.LogWarning(error);
                        throw new NotImplementedException(error);
                }

                if (dbupResult != null && !dbupResult.Successful)
                {
                    var message = $"DBUP failed with error: '{dbupResult?.Error?.Message}' - '{dbupResult?.Error?.ToString()}'";
                    logger.LogCritical(message);
                    Environment.FailFast(message);
                }

                if (config.GetOrDefault(Settings.Database.SyncFromConfig, false))
                { 
                    logger.LogWarning($"Sync_From_Config set to true, wiping settings database and re-pulling values from config");
                    await database.DeleteAllSettings();
                }

                var isDemoMode = config.GetOrDefault(Settings.IsDemoMode, false);
                await database.SetSetting(new SettingModel()
                {
                    Id = Settings.IsDemoMode.ToUpper(),
                    Value = isDemoMode.ToString()
                });

                await database.SetSetting(new SettingModel()
                {
                    Id = Settings.Themes.Default.ToUpper(),
                    Value = config.GetOrDefault(Settings.Themes.Default, Themes.AutoDetect.ToString())
                });

                var username = !isDemoMode ? config.GetOrDefault(Settings.Account.Admin.Username, config.GetOrDefault(Settings.Account.Owner.Username, "admin")).ToLower() : "demo";
                var ownerAccount = new UserModel() 
                {
                    Username = username,
                    Password = encryption.Encrypt(!isDemoMode ? config.GetOrDefault(Settings.Account.Admin.Password, config.GetOrDefault(Settings.Account.Owner.Password, "admin")) : "demo", username)
                };
                await database.InitOwnerAccount(ownerAccount);

                await new DbupImporter(config, database, loggerFactory.CreateLogger<DbupImporter>()).ImportSettings();

                if (config.GetOrDefault(Settings.Account.Owner.LogPassword, false))
                {
                    logger.LogInformation($"Password: {ownerAccount.Password}");
                }

                if (config.GetOrDefault(Security.MultiFactor.ResetToDefault, false))
                {
                    await database.ResetMultiFactorToDefault();
                }
            }
            else
            {
                logger.LogError($"DBUP failed with error: 'Connection string was null or empty'");
                throw new ArgumentNullException("Please specify a valid database connection string");
            }
        }
    }

    public class DbupSqliteHelper
    {
        public DatabaseUpgradeResult Migrate(string connectionString)
        {
            try
            {
                var dbupBuilder = DeployChanges.To
                    .SqliteDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .WithScriptNameComparer(new DbupScriptComparer())
                    .WithFilter(new DbupScriptFilter(DatabaseType.SQLite))
                    .LogToConsole();
                dbupBuilder.Configure(c => c.Journal = new DbupSQLiteTableJournal(() => c.ConnectionManager, () => c.Log, "schemaversions"));

                return dbupBuilder.Build().PerformUpgrade();
            }
            catch (Exception ex)
            {
                return new DatabaseUpgradeResult(null, false, ex, null);
            }
        }
    }

    public class DbupMySqlHelper
    {
        public DatabaseUpgradeResult Migrate(string connectionString, string database)
        {
            try
            {
                database = !string.IsNullOrWhiteSpace(database) ? database : Regex.Match(connectionString, "Database\\=(.+?)(;|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups[1].Value;

                var dbupBuilder = DeployChanges.To
                    .MySqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .WithScriptNameComparer(new DbupScriptComparer())
                    .WithFilter(new DbupScriptFilter(DatabaseType.MySQL))
                    .LogToConsole();
                dbupBuilder.Configure(c => c.Journal = new DbupMySqlTableJournal(() => c.ConnectionManager, () => c.Log, database, "schemaversions"));

                return dbupBuilder.Build().PerformUpgrade();
            }
            catch (Exception ex)
            {
                return new DatabaseUpgradeResult(null, false, ex, null);
            }
        }
    }
}