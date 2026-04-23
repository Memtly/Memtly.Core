namespace Memtly.Core.Models
{
    public class RegisterModel
    {
        public RegisterModel()
        {
            this.Username = string.Empty;
            this.Firstname = string.Empty;
            this.Lastname = string.Empty;
            this.EmailAddress = string.Empty;
            this.Password = string.Empty;
            this.ConfirmPassword = string.Empty;
        }

        public string? Username { get; set; }
        public string? Firstname{ get; set; }
        public string? Lastname { get; set; }
        public string? EmailAddress { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}