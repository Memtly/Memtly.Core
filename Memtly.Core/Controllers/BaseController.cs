using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using Memtly.Core.Models;

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
                                using (var ms = new MemoryStream())
                                {
                                    using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                                    {
                                        foreach (var file in files)
                                        {
                                            var path = Path.GetRelativePath(contents.SourcePath, file);
                                            var archiveEntry = archive.CreateEntry(path);

                                            using (var es = archiveEntry.Open())
                                            using (var fs = System.IO.File.OpenRead(file))
                                            {
                                                await fs.CopyToAsync(es);
                                            }
                                        }

                                    }

                                    var relativePath = $"{contents.FileName.TrimStart('/')}";
                                    var zipEntry = zipArchive.CreateEntry(!string.IsNullOrWhiteSpace(contents.Directory) ? Path.Combine(contents.Directory, relativePath) : relativePath);

                                    using (var entryStream = zipEntry.Open())
                                    {
                                        ms.Seek(0, SeekOrigin.Begin);
                                        await ms.CopyToAsync(entryStream);
                                    }
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

            return new EmptyResult();
        }
    }
}