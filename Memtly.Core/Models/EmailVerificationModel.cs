namespace Memtly.Core.Models
{
    public class EmailVerificationModel
    {
        public EmailVerificationModel()
        {
            this.Username = string.Empty;
            this.Validator = string.Empty;
        }

        public string Username { get; set; }
        public string Validator { get; set; }
    }
}