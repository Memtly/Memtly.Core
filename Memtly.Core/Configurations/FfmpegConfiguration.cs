using Microsoft.Extensions.Localization;
using Memtly.Core.Constants;
using Memtly.Core.Helpers;

namespace Memtly.Core.Configurations
{
    public static class FfmpegConfiguration
    {
        public static void AddFfmpegConfiguration(this IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfigHelper>();
            var localizer = services.BuildServiceProvider().GetRequiredService<IStringLocalizer<Localization.Translations>>();
            var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();

            var ffmpegPath = config.GetOrDefault(FFMPEG.InstallPath, "/ffmpeg");

            var downloaded = new ImageHelper(new FileHelper(loggerFactory.CreateLogger<FileHelper>()), loggerFactory.CreateLogger<ImageHelper>(), localizer).DownloadFFMPEG(ffmpegPath).Result;
            if (!downloaded)
            {
                loggerFactory.CreateLogger<FFMPEG>().LogWarning($"{localizer["FFMPEG_Download_Failed"].Value} '{ffmpegPath}'");
            }
        }
    }
}