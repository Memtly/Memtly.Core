using System.IO.Compression;
using Memtly.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Memtly.Core.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
            : base()
        {
        }

        protected async Task<IActionResult> ZipFileResponse(string filename, ZipListing content)
        {
            return await ZipFileResponse(filename, new List<ZipListing>() { content });
        }

        protected async Task<IActionResult> ZipFileResponse(string filename, IEnumerable<ZipListing> contentsList)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filename) && contentsList != null && contentsList.Count() > 0)
                {
                    HttpContext.Response.Headers.Append("Content-Type", "application/zip");
                    HttpContext.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{filename}\"");

                    var bodyStream = Response.BodyWriter.AsStream(true);

                    using (var zipArchive = new ZipArchive(bodyStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var contents in contentsList.Where(x => !string.IsNullOrWhiteSpace(x.SourcePath)))
                        {
                            var files = contents?.Files?.Where(x => x.StartsWith(contents.SourcePath, StringComparison.OrdinalIgnoreCase));
                            if (files != null && files.Any())
                            {
                                if (!string.IsNullOrWhiteSpace(contents?.FileName))
                                {
                                    var tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.zip");

                                    try
                                    {
                                        await using (var tempFileStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 81920, FileOptions.Asynchronous))
                                        {
                                            using (var archive = new ZipArchive(tempFileStream, ZipArchiveMode.Create, true))
                                            {
                                                foreach (var file in files)
                                                {
                                                    var path = Path.GetRelativePath(contents.SourcePath, file);
                                                    var archiveEntry = archive.CreateEntry(path);

                                                    await using (var es = archiveEntry.Open())
                                                    await using (var fs = System.IO.File.OpenRead(file))
                                                    {
                                                        await fs.CopyToAsync(es);
                                                    }
                                                }
                                            }
                                        }

                                        var relativePath = contents.FileName.TrimStart('/');
                                        var zipEntry = zipArchive.CreateEntry(!string.IsNullOrWhiteSpace(contents.Directory) ? Path.Combine(contents.Directory, relativePath) : relativePath);

                                        await using (var entryStream = zipEntry.Open())
                                        await using (var tempReadStream = new FileStream(tempZipPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
                                        {
                                            await tempReadStream.CopyToAsync(entryStream);
                                        }
                                    }
                                    finally
                                    {
                                        try
                                        {
                                            if (System.IO.File.Exists(tempZipPath))
                                            {
                                                System.IO.File.Delete(tempZipPath);
                                            }
                                        }
                                        catch { }
                                    }
                                }
                                else
                                {
                                    foreach (var file in files)
                                    {
                                        var relativePath = Path.GetRelativePath(contents.SourcePath, file);
                                        var zipEntry = zipArchive.CreateEntry(!string.IsNullOrWhiteSpace(contents.Directory) ? Path.Combine(contents.Directory, relativePath) : relativePath);

                                        using (var fs = System.IO.File.OpenRead(file))
                                        using (var entryStream = zipEntry.Open())
                                        {
                                            await fs.CopyToAsync(entryStream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetService<ILogger<BaseController>>();
                if (logger != null)
                { 
                    var localizer = HttpContext.RequestServices.GetService<IStringLocalizer<Localization.Translations>>();
                    if (localizer != null)
                    { 
                        logger.LogWarning(ex, $"{localizer["Unexpected_Error_Occurred"].Value} - {ex?.Message}");
                    }
                }
            }

            return new EmptyResult();
        }
    }
}