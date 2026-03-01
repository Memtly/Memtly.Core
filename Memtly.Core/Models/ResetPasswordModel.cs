namespace Memtly.Core.Models
{
    public class ResetPasswordModel
    {
        public ResetPasswordModel()
        {
            this.Data = string.Empty;
            this.Password = string.Empty;
            this.ConfirmPassword = string.Empty;
        }

        public string? Data { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}