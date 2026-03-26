using System.Security.Cryptography;
using System.Text;
using Memtly.Core.Constants;

namespace Memtly.Core.Helpers
{
    public interface IEncryptionHelper
    {
        bool IsEncryptionEnabled();
        string Encrypt(string value, string? salt = null);
    }

    public class EncryptionHelper : EncodingHelper, IEncryptionHelper
    {
        private readonly HashAlgorithmName _hashType;
        private readonly int _iterations;
        private readonly string _key;
        private readonly string _salt;

        public EncryptionHelper(ISettingsHelper settings)
            : base()
        {
            _hashType = ParseHashType(settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.HashType, "SHA256").Result);
            _iterations = settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Iterations, 1000).Result;
            
            _key = settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Key, string.Empty).Result;
            _salt = settings.GetOrDefault(MemtlyConfiguration.Security.Encryption.Salt, "WUtlVOvC2a6ol9M6ZidO5sJkQxYMolyasFid2Fyqvjd0uucAjYy5EsHPxdeplFRj").Result;
        }

        public bool IsEncryptionEnabled()
        {
            return !string.IsNullOrWhiteSpace(_key) && !string.IsNullOrWhiteSpace(_salt);
        }

        public string Encrypt(string value, string? salt = null)
        {
            var enabled = this.IsEncryptionEnabled();
            if (enabled)
            { 
                var clearBytes = Encoding.Unicode.GetBytes(value);
                var saltBytes = Encoding.Unicode.GetBytes(salt ?? _salt);

                using (var encryptor = Aes.Create())
                {
                    var pdb = new Rfc2898DeriveBytes(_key, saltBytes, _iterations, _hashType);

                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);

                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }

                        value = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }

            return value;
        }

        private HashAlgorithmName ParseHashType(string name)
        {
            switch (name?.Trim()?.ToUpper())
            {
                case "MD5":
                    return HashAlgorithmName.MD5;
                case "SHA1":
                    return HashAlgorithmName.SHA1;
                case "SHA256":
                    return HashAlgorithmName.SHA256;
                case "SHA384":
                    return HashAlgorithmName.SHA384;
                case "SHA512":
                    return HashAlgorithmName.SHA512;
                case "SHA3_256":
                    return HashAlgorithmName.SHA3_256;
                case "SHA3_384":
                    return HashAlgorithmName.SHA3_384;
                case "SHA3_512":
                    return HashAlgorithmName.SHA3_512;
                default:
                    return HashAlgorithmName.SHA256;
            }
        }
    }
}