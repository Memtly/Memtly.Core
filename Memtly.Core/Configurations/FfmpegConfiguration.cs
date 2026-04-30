using Microsoft.Extensions.Localization;
using Memtly.Core.Constants;
using Memtly.Core.Helpers;

namespace Memtly.Core.Configurations
{
    public static class FfmpegConfiguration
    {
        public static void AddFfmpegConfiguration(this IServiceCollection services)
        {
            var bsp = services.BuildServiceProvider();
            var config = bsp.GetRequiredService<IConfigHelper>();
            var localizer = bsp.GetRequiredService<IStringLocalizer<Localization.Translations>>();
            var loggerFactory = bsp.GetRequiredService<ILoggerFactory>();

            var ffmpegPath = config.GetOrDefault(FFMPEG.InstallPath, "/ffmpeg");

            var downloaded = new ImageHelper(new FileHelper(loggerFactory.CreateLogger<FileHelper>()), loggerFactory.CreateLogger<ImageHelper>(), localizer).DownloadFFMPEG(ffmpegPath).Result;
            if (!downloaded)
            {
                loggerFactory.CreateLogger<FFMPEG>().LogWarning($"{localizer["FFMPEG_Download_Failed"].Value} '{ffmpegPath}'");
            }
        }
    }
}