using System.Text.Json.Serialization;

namespace Memtly.Core.Models
{
    public class LoginModel
    {
        public LoginModel()
        {
            this.Username = string.Empty;
            this.Password = string.Empty;
            this.Code = string.Empty;
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
    }

    public class LoginResponse
    {
        public LoginResponse(bool success)
        {
            Success = success;
        }

        [JsonPropertyName("success")]
        public bool Success { get; set; } = false;

        [JsonPropertyName("mfa")]
        public bool MFAEnabled { get; set; } = false;

        [JsonPropertyName("pending_activation")]
        public bool PendingActivation { get; set; } = false;
    }
}