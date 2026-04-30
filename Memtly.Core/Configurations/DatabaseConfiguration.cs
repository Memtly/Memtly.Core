using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Memtly.Core.Constants;
using Memtly.Core.EntityFramework;
using Memtly.Core.Enums;
using Memtly.Core.Helpers;
using Memtly.Core.Helpers.Database;
using Memtly.Core.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Memtly.Core.Configurations
{
    public static class DatabaseConfiguration
    {
        public static void AddDatabaseConfiguration(this IServiceCollection services)
        {
            var bsp = services.BuildServiceProvider();
            var config = bsp.GetRequiredService<IConfigHelper>();
            
            var provider = config.GetOrDefault(MemtlyConfiguration.Database.Type, "sqlite");
            var connString = config.GetOrDefault(MemtlyConfiguration.Database.ConnectionString, "Data Source=./config/memtly.db");
            var assemblyName = typeof(CoreDbContext).Assembly.GetName().Name;

            if (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var fileHelper = bsp.GetRequiredService<IFileHelper>();
                    var databasePathMatch = Regex.Match(connString, "Data Source=(.+?)(;|$)", RegexOptions.Multiline);
                    if (databasePathMatch?.Groups != null && databasePathMatch.Groups.Count == 3)
                    {
                        fileHelper.CreateDirectoryIfNotExists(Path.GetDirectoryName(databasePathMatch.Groups[1].Value)!);
                    }
                }
                catch { }
            }

            services.AddDbContext<CoreDbContext>(options =>
            {
                MemtlyCore.DatabaseType = provider.ToLower();
                switch (provider.ToLower())
                {
                    case "sqlite":
                        options.UseSqlite(connString, x =>
                        {
                            x.MigrationsAssembly(assemblyName);
                            x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                        });
                        break;
                    case "mysql":
                    case "mariadb":
                        options.UseMySql(connString, ServerVersion.AutoDetect(connString), x =>
                        {
                            x.MigrationsAssembly(assemblyName);
                            x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                        });
                        break;
                    case "mssql":
                        options.UseSqlServer(connString, x =>
                        {
                            x.MigrationsAssembly(assemblyName);
                            x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                        });
                        break;
                    case "postgres":
                        options.UseNpgsql(connString, x =>
                        {
                            x.MigrationsAssembly(assemblyName);
                            x.MigrationsHistoryTable($"__EFMigrationsHistory_{provider}");
                        });
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported database provider: '{provider}'. Supported: sqlite, mysql, mariadb, mssql, postgres");
                }

                options.ReplaceService<IMigrationsAssembly, CoreDbContextMigrationFilter>();
            });

            services.AddScoped<IDatabaseHelper, EFDatabaseHelper>();

            bsp = services.BuildServiceProvider();
            var ctx = bsp.GetRequiredService<CoreDbContext>();

            var dbProvider = ctx.Database.ProviderName;

            ctx.Database.Migrate();

            var encryption = bsp.GetRequiredService<IEncryptionHelper>();
            var logger = bsp.GetRequiredService<ILogger<EFDatabaseHelper>>();

            using (var scope = bsp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                logger.LogInformation($"Initializing database - {MemtlyCore.DatabaseType}");
                InitializeDatabase(config, db, encryption, logger);
                logger.LogInformation($"Initialization complete");
            }
        }

        private static void InitializeDatabase(IConfigHelper config, IDatabaseHelper database, IEncryptionHelper encryption, ILogger logger)
        {
            var isDemoMode = config.GetOrDefault(MemtlyConfiguration.IsDemoMode, false);
            var password = encryption.Encrypt(!isDemoMode ? config.GetOrDefault(MemtlyConfiguration.Account.Admin.Password, config.GetOrDefault(MemtlyConfiguration.Account.Admin.Password, "admin")) : "demo", UserAccounts.AdminUser.ToLower());
            var allowInsecureGalleries = config.GetOrDefault(MemtlyConfiguration.Security.Hardening.AllowInsecureGalleries, true);
            var defaultSecretKey = config.GetOrDefault(MemtlyConfiguration.Basic.DefaultGallerySecretKey, string.Empty);

            Task.Run(async () =>
            {
                var systemAccount = await database.GetUserByUsername(UserAccounts.SystemUser);
                if (systemAccount == null)
                {
                    await database.AddUser(new UserModel
                    {
                        Username = UserAccounts.SystemUser.ToLower(),
                        Email = $"system@example.com",
                        Firstname = "System",
                        Lastname = "User",
                        Password = PasswordHelper.GenerateTempPassword(),
                        State = AccountState.Active,
                        Level = UserLevel.System
                    });
                }
                else
                {
                    systemAccount.Firstname = "System";
                    systemAccount.Lastname = "User";
                    systemAccount.State = AccountState.Active;
                    systemAccount.Level = UserLevel.System;

                    await database.EditUser(systemAccount);
                }

                var adminAccount = await database.GetUserByUsername(UserAccounts.AdminUser);
                if (adminAccount == null)
                {
                    await database.AddUser(new UserModel
                    {
                        Username = UserAccounts.AdminUser.ToLower(),
                        Email = config.GetOrDefault(MemtlyConfiguration.Account.Admin.EmailAddress, "admin@example.com"),
                        Firstname = "Admin",
                        Lastname = "User",
                        Password = password,
                        State = AccountState.Active,
                        Level = UserLevel.Admin
                    });
                }
                else
                {
                    adminAccount.Email = config.GetOrDefault(MemtlyConfiguration.Account.Admin.EmailAddress, "admin@example.com");
                    adminAccount.Firstname = "Admin";
                    adminAccount.Lastname = "User";
                    adminAccount.Password = password;
                    adminAccount.State = AccountState.Active;
                    adminAccount.Level = UserLevel.Admin;

                    await database.EditUser(adminAccount);
                    await database.ChangePassword(adminAccount);
                }

                adminAccount = await database.GetUserByUsername(UserAccounts.AdminUser);
                if (adminAccount != null)
                {
                    var defaultGalleryId = await database.GetGalleryId(SystemGalleries.DefaultGallery.ToLower());
                    var defaultGallery = defaultGalleryId != null ? await database.GetGallery(defaultGalleryId.Value) : null;

                    if (defaultGallery == null)
                    {
                        var secretKey = !string.IsNullOrWhiteSpace(defaultSecretKey) || allowInsecureGalleries ? defaultSecretKey : PasswordHelper.GenerateGallerySecretKey();
                        await database.AddGallery(new GalleryModel
                        {
                            Identifier = SystemGalleries.DefaultGallery.ToLower(),
                            Name = SystemGalleries.DefaultGallery,
                            SecretKey = secretKey,
                            Owner = adminAccount.Id
                        });
                    }
                }

                if (config.GetOrDefault(MemtlyConfiguration.Database.SyncFromConfig, false))
                {
                    logger.LogWarning($"Sync_From_Config set to true, wiping settings database and re-pulling values from config");
                    await database.DeleteAllSettings();
                }

                await ImportSettings(config, database, logger);

                await database.SetSetting(new SettingModel()
                {
                    Id = MemtlyConfiguration.IsDemoMode.ToUpper(),
                    Value = isDemoMode.ToString()
                });

                await database.SetSetting(new SettingModel()
                {
                    Id = MemtlyConfiguration.Themes.Default.ToUpper(),
                    Value = config.GetOrDefault(MemtlyConfiguration.Themes.Default, Themes.AutoDetect.ToString())
                });

                if (config.GetOrDefault(MemtlyConfiguration.Security.MultiFactor.ResetToDefault, false))
                {
                    await database.ResetMultiFactorToDefault();
                }
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task ImportSettings(IConfigHelper config, IDatabaseHelper database, ILogger logger)
        {
            try
            {
                var galleries = (await database.GetGalleries())?.Where(x => !x.Identifier.Equals(SystemGalleries.AllGallery, StringComparison.OrdinalIgnoreCase));

                var settings = await database.GetAllSettings();
                if (settings == null || !settings.Any(setting => setting.Id.StartsWith(MemtlyConfiguration.Basic.BaseKey, StringComparison.OrdinalIgnoreCase)))
                {
                    var systemKeys = GetAllKeys().Where(x => !x.StartsWith(MemtlyConfiguration.Gallery.BaseKey, StringComparison.OrdinalIgnoreCase));
                    foreach (var key in systemKeys)
                    {
                        try
                        {
                            if (settings == null || !settings.Any(setting => setting.Id.Equals(key, StringComparison.OrdinalIgnoreCase)))
                            {
                                var configVal = config.Get(key);
                                if (!string.IsNullOrWhiteSpace(configVal))
                                {
                                    await database.AddSetting(new SettingModel()
                                    {
                                        Id = key,
                                        Value = configVal
                                    });
                                }
                            }
                        }
                        catch { }
                    }

                    if (galleries != null && galleries.Any())
                    {
                        var galleryKeys = GetKeys<MemtlyConfiguration.Gallery>();
                        foreach (var gallery in galleries)
                        {
                            if (!string.IsNullOrWhiteSpace(gallery?.Name))
                            {
                                foreach (var key in galleryKeys)
                                {
                                    try
                                    {
                                        var galleryOverride = config.GetEnvironmentVariable(key, gallery.Name);
                                        if (!string.IsNullOrWhiteSpace(galleryOverride))
                                        {
                                            await database.AddSetting(new SettingModel()
                                            {
                                                Id = key,
                                                Value = galleryOverride
                                            }, gallery.Id);
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }

                // Protect any galleries without a secret key by forcing a new one
                if (galleries != null && galleries.Any())
                {
                    var allowInsecureGalleries = config.GetOrDefault(MemtlyConfiguration.Security.Hardening.AllowInsecureGalleries, true);
                    if (!allowInsecureGalleries)
                    {
                        foreach (var gallery in galleries.Where(gallery => string.IsNullOrWhiteSpace(gallery.SecretKey)))
                        {
                            try
                            {
                                gallery.SecretKey = config.GetOrDefault(MemtlyConfiguration.Basic.DefaultGallerySecretKey, allowInsecureGalleries ? string.Empty : PasswordHelper.GenerateGallerySecretKey());

                                await database.EditGallery(gallery);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to import settings at startup - {ex?.Message}", ex);
            }
        }

        private static IEnumerable<string> GetAllKeys()
        {
            var keys = new List<string>();

            try
            {
                keys.AddRange(GetKeys<MemtlyConfiguration.Account>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Alerts>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Audit>());
                keys.AddRange(GetKeys<MemtlyConfiguration.BackgroundServices>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Basic>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Database>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Gallery>());
                keys.AddRange(GetKeys<MemtlyConfiguration.GallerySelector>());
                keys.AddRange(GetKeys<MemtlyConfiguration.IdentityCheck>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Languages>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Notifications>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Policies>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Reports>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Security>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Slideshow>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Sponsors>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Themes>());
                keys.AddRange(GetKeys<MemtlyConfiguration.Trackers>());
            }
            catch { }

            return keys.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct();
        }

        private static IEnumerable<string> GetKeys<T>(bool includeNesteted = true)
        {
            var keys = new List<string>();

            try
            {
                var obj = Activator.CreateInstance<T>();
                foreach (var val in GetConstants(typeof(T), includeNesteted))
                {
                    keys.Add((string)(val.GetValue(obj) ?? string.Empty));
                }
            }
            catch { }

            return keys.Where(x => !string.IsNullOrWhiteSpace(x) && !x.EndsWith(':'));
        }

        private static FieldInfo[] GetConstants(Type type, bool includeNesteted)
        {
            var constants = new ArrayList();

            try
            {
                if (includeNesteted)
                {
                    var classInfos = type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    foreach (var ci in classInfos)
                    {
                        var consts = GetConstants(ci, includeNesteted);
                        if (consts != null && consts.Length > 0)
                        {
                            constants.AddRange(consts);
                        }
                    }
                }

                var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                foreach (var fi in fieldInfos)
                {
                    if (fi.IsLiteral && !fi.IsInitOnly)
                    {
                        constants.Add(fi);
                    }
                }
            }
            catch { }

            return (FieldInfo[])constants.ToArray(typeof(FieldInfo));
        }
    }
}