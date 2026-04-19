using System.Reflection;
using Memtly.Core.BackgroundWorkers;
using Memtly.Core.Configurations;
using Memtly.Core.Constants;
using Memtly.Core.Helpers;
using Memtly.Core.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;

namespace Memtly.Core.Extensions
{
    public static class StartupExtensions
    {
        public static void ConfigureCommunityServices(this IServiceCollection services)
        {
            services.ConfigureSharedServices();
        }

        public static void ConfigureEnterpriseServices(this IServiceCollection services)
        {
            services.ConfigureSharedServices();
        }

        public static void ConfigureSharedServices(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddDependencyInjectionConfiguration();
            services.AddDatabaseConfiguration();
            services.AddWebClientConfiguration();
            services.AddNotificationConfiguration();
            services.AddLocalizationConfiguration();
            services.AddFfmpegConfiguration();

            services.AddHostedService<DirectoryScanner>();
            services.AddHostedService<NotificationReport>();
            services.AddHostedService<CleanupService>();

            services.AddResponseCaching();
            services.AddRazorPages();
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddApplicationPart(typeof(Memtly.Core.MemtlyCore).Assembly);

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
            });

            services.Configure<FormOptions>(x =>
            {
                x.MultipartHeadersLengthLimit = Int32.MaxValue;
                x.MultipartBoundaryLengthLimit = Int32.MaxValue;
                x.MultipartBodyLengthLimit = Int64.MaxValue;
                x.ValueLengthLimit = Int32.MaxValue;
                x.BufferBodyLengthLimit = Int64.MaxValue;
                x.MemoryBufferThreshold = Int32.MaxValue;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = false;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);

                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = $"/Error?Reason={ErrorCode.Unauthorized}";
                    options.SlidingExpiration = true;
                });
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.Name = ".Memtly.Session";
                options.Cookie.IsEssential = true;
            });
            services.AddDataProtection()
                .SetApplicationName("Memtly")
                .PersistKeysToFileSystem(new DirectoryInfo(Directories.Private.Config));

            services.AddRequestTimeouts(options =>
            {
                options.AddPolicy("timeout_1h", TimeSpan.FromHours(1));
            });
        }

        public static void ConfigureCommunity(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ConfigureShared(env);
        }

        public static void ConfigureEnterprise(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ConfigureShared(env);
        }

        public static void ConfigureShared(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            var config = app.ApplicationServices.GetRequiredService<IConfigHelper>();
            var settings = app.ApplicationServices.GetRequiredService<ISettingsHelper>();
            var logger = app.ApplicationServices.GetRequiredService<ILogger<MemtlyCore>>();

            logger.LogInformation($"Release Version - '{settings.GetReleaseVersion(4)}'");

            app.UseExceptionHandler();

            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            if (settings.GetOrDefault(MemtlyConfiguration.Basic.ForceHttps, false).Result)
            {
                app.UseHttpsRedirection();
            }

            app.UseCookiePolicy();

            foreach (var dirName in $"{Directories.Public.Uploads},{Directories.Public.CustomResources},{Directories.Public.Thumbnails},{Directories.Public.TempFiles}".Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var dirPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, dirName);
                Directory.CreateDirectory(dirPath);

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(dirPath),
                    RequestPath = $"/{dirName}"
                });
            }

            foreach (var dirName in $"{Directories.Private.Config}".Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var dirPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, dirName);
                Directory.CreateDirectory(dirPath);
            }

            if (config.GetOrDefault(MemtlyConfiguration.Security.Headers.Enabled, true))
            {
                try
                {
                    var baseUrl = settings.GetOrDefault(MemtlyConfiguration.Basic.BaseUrl, string.Empty).Result;
                    var baseUrlCSP = "http://localhost:* ws://localhost:*";
                    if (!string.IsNullOrWhiteSpace(baseUrl))
                    {
                        try
                        {
                            var uri = new Uri(baseUrl);
                            baseUrlCSP = !string.IsNullOrWhiteSpace(uri.Host) ? $"{uri.Scheme}://{uri.Host}:* {(uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws")}://{uri.Host}:*" : string.Empty;
                        }
                        catch { }
                    }

                    var umamiUrl = settings.GetOrDefault(MemtlyConfiguration.Trackers.Umami.Endpoint, string.Empty).Result;
                    var trackersUrlCSP = string.Empty;
                    if (!string.IsNullOrWhiteSpace(umamiUrl))
                    {
                        try
                        {
                            var uri = new Uri(umamiUrl);
                            if (!string.IsNullOrWhiteSpace(uri.Host))
                            {
                                trackersUrlCSP = $"{trackersUrlCSP} {uri.Scheme}://{uri.Host}:*".Trim();
                            }
                        }
                        catch { }
                    }

                    app.Use(async (context, next) =>
                    {
                        context.Response.Headers.Remove("X-Frame-Options");
                        context.Response.Headers.Append("X-Frame-Options", config.GetOrDefault(MemtlyConfiguration.Security.Headers.XFrameOptions, "SAMEORIGIN"));

                        context.Response.Headers.Remove("X-Content-Type-Options");
                        context.Response.Headers.Append("X-Content-Type-Options", config.GetOrDefault(MemtlyConfiguration.Security.Headers.XContentTypeOptions, "nosniff"));

                        context.Response.Headers.Remove("Content-Security-Policy");
                        context.Response.Headers.Append("Content-Security-Policy", config.GetOrDefault(MemtlyConfiguration.Security.Headers.CSP, $"default-src 'self' {(!string.IsNullOrWhiteSpace(baseUrlCSP) ? baseUrlCSP : "http://localhost:* ws://localhost:*")}; script-src 'self' 'unsafe-inline' 'unsafe-eval'{(!string.IsNullOrWhiteSpace(trackersUrlCSP) ? $" {trackersUrlCSP}" : string.Empty)}; style-src 'self' 'unsafe-inline'; connect-src 'self' {(!string.IsNullOrWhiteSpace(baseUrlCSP) ? baseUrlCSP : "http://localhost:* ws://localhost:*")}{(!string.IsNullOrWhiteSpace(trackersUrlCSP) ? $" {trackersUrlCSP}" : string.Empty)}; font-src 'self'; img-src 'self' https://github.com/ https://avatars.githubusercontent.com/ data:; frame-src 'self'; frame-ancestors 'self';"));

                        await next();
                    });
                }
                catch { }
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRequestLocalization();
            app.UseSession();
            app.UseRequestTimeouts();
            app.UseResponseCaching();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
                endpoints.MapRazorPages();
            });
        }
    }
}