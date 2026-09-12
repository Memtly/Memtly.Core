using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Memtly.Core.Extensions;

namespace Memtly.Core.Helpers
{
    public interface IFileHelper
    {
        bool DirectoryExists(string path);
        bool CreateDirectoryIfNotExists(string path);
        bool MoveDirectoryIfExists(string path, string newPath);
        bool DeleteDirectoryIfExists(string path, bool recursive = true);
        bool PurgeDirectory(string path);
        string[] GetDirectories(string path, string pattern = "*", SearchOption searchOption = SearchOption.AllDirectories);
        string[] GetFiles(string path, string pattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories);
        bool FileExists(string path);
        long FileSize(string path);
        bool DeleteFileIfExists(string path);
        bool CopyFileIfExists(string source, string destination);
        bool MoveFileIfExists(string source, string destination);
        long GetDirectorySize(string path);
        Task<byte[]> ReadAllBytes(string path);
        Task SaveFile(IFormFile file, string path, FileMode mode);
        Task<string> GetChecksum(string path);
        Task<DateTime> GetCreationDatetime(string path);
        string BytesToHumanReadable(long bytes, int decimalPlaces = 0);
        string SanitizeFilename(string filename);
        bool IsCleanPath(string path);
    }

    public class FileHelper : IFileHelper
    {
        private readonly ILogger<FileHelper> _logger;

        public FileHelper(ILogger<FileHelper> logger)
        {
            _logger = logger;
        }

        public bool DirectoryExists(string path)
        {
            return IsCleanPath(path) && Directory.Exists(path);
        }

        public bool CreateDirectoryIfNotExists(string path)
        {
            if (IsCleanPath(path) && !DirectoryExists(path))
            {
                if (FileExists(path))
                {
                    _logger.LogWarning($"Failed to create directory '{path}' as a file with the same name already exists. If this is Linux you should delete this file to allow the directory creation.");
                    return false;
                }

                Directory.CreateDirectory(path);

                return true;
            }
                
            return false;
        }

        public bool MoveDirectoryIfExists(string path, string newPath)
        {
            if (IsCleanPath(path) && IsCleanPath(newPath) && DirectoryExists(path))
            {
                Directory.Move(path, newPath);

                return true;
            }

            return false;
        }

        public bool DeleteDirectoryIfExists(string path, bool recursive = true)
        {
            if (IsCleanPath(path) && DirectoryExists(path))
            {
                Directory.Delete(path, recursive);

                return true;
            }

            return false;
        }

        public bool PurgeDirectory(string path)
        {
            if (IsCleanPath(path))
            { 
                DeleteDirectoryIfExists(path);
                return CreateDirectoryIfNotExists(path);
            }

            return false;
        }

        public string[] GetDirectories(string path, string pattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (IsCleanPath(path))
            {
                return Directory.GetDirectories(path, pattern, searchOption);
            }

            return new string[0];
        }

        public string[] GetFiles(string path, string pattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (IsCleanPath(path))
            {
                return Directory.GetFiles(path, pattern, searchOption);
            }

            return new string[0];
        }

        public bool FileExists(string path)
        {
            if (IsCleanPath(path))
            {
                return File.Exists(path);
            }

            return false;
        }

        public long FileSize(string path)
        {
            if (IsCleanPath(path))
            {
                return new FileInfo(path).Length;
            }

            return 0;
        }

        public bool DeleteFileIfExists(string path)
        {
            if (IsCleanPath(path) && FileExists(path))
            {
                File.Delete(path);

                return true;
            }

            return false;
        }

        public bool CopyFileIfExists(string source, string destination)
        {
            if (IsCleanPath(source) && IsCleanPath(destination) && FileExists(source))
            {
                File.Copy(source, destination);

                return true;
            }

            return false;
        }

        public bool MoveFileIfExists(string source, string destination)
        {
            if (IsCleanPath(source) && IsCleanPath(destination) && FileExists(source))
            {
                File.Move(source, destination);

                return true;
            }

            return false;
        }

        public long GetDirectorySize(string path)
        {
            long size = 0;

            if (IsCleanPath(path) && DirectoryExists(path))
            {
                var info = new DirectoryInfo(path);
                
                foreach (var file in info.GetFiles())
                {      
                    size += file.Length;    
                }
                
                foreach (var dir in info.GetDirectories())
                {
                    size += GetDirectorySize(dir.FullName);
                }
            }

            return size;
        }

        public async Task<byte[]> ReadAllBytes(string path)
        {
            if (IsCleanPath(path))
            {
                return await File.ReadAllBytesAsync(path);
            }

            return await Task.FromResult(new byte[0]);
        }

        public async Task SaveFile(IFormFile file, string path, FileMode mode)
        {
            if (IsCleanPath(path))
            {
			    using (var fs = new FileStream(path, mode))
			    {
				    await file.CopyToAsync(fs);
			    }
            }
		}

        public async Task<string> GetChecksum(string path)
        {
            if (!IsCleanPath(path))
            {
                return await Task.Run(() => 
                {
                    var checksum = string.Empty;

                    try
                    {
                        using (var sha256 = SHA256.Create())
                        using (var stream = File.OpenRead(path))
                        {
                            var hashBytes = sha256.ComputeHash(stream);
                            checksum = Convert.ToHexString(hashBytes).ToLowerInvariant();
                        }
                    }
                    catch (Exception ex) 
                    {
                        _logger.LogWarning(ex, $"Failed to compute MD5 checksum for file '{path}'");
                    }

                    return checksum.RemoveNullBytes();
                });
            }

            return await Task.FromResult(string.Empty);
        }

        public async Task<DateTime> GetCreationDatetime(string path)
        {
            if (IsCleanPath(path))
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        var fsTime = File.GetLastWriteTimeUtc(path);
                        var creationTime = File.GetCreationTimeUtc(path);

                        return creationTime < fsTime ? creationTime : fsTime;
                    }
                    catch
                    {
                        return DateTime.UtcNow;
                    }
                });
            }

            return await Task.FromResult(DateTime.UtcNow);
        }

        public string BytesToHumanReadable(long bytes, int decimalPlaces = 0)
        {
            var sizes = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            var place = 0;
            var total = 0.0;

            var decimalFormat = "###0.";
            for (var i = 0; i < decimalPlaces; i++)
            {
                decimalFormat += "0";
            }

            if (bytes >= 0)
            { 
                try
                {
                    long b = Math.Abs(bytes);
                    place = Convert.ToInt32(Math.Floor(Math.Log(b ,1000)));
                    double num = Math.Round(b / Math.Pow(1000, place), 2);
                    total = Math.Sign(bytes) * num;
                }
                catch { }
            }

            return total.ToString($"{decimalFormat.TrimEnd('.')} {sizes[place]}");
        }

        public string SanitizeFilename(string filename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var regex = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(filename, regex, string.Empty, RegexOptions.Compiled);
        }

        public bool IsCleanPath(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)
                    || path.Contains("..")
                    || path.Contains('\0')
                    || path.EndsWith('.')
                    || path.EndsWith(' ')
                ) {
                    _logger.LogWarning($"A file action was blocked as it attempted to access a dirty path. Path: '{path}'");
                    return false;
                }
            }
            catch 
            {
                _logger.LogWarning($"An exception was thrown while checking if a file path was malicious. For safety the action was rejected. Path: '{path}'");
                return false;
            }

            return true;
        }
    }
}