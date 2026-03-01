using System.Net.Mail;

namespace Memtly.Core.Helpers
{
    public interface ISmtpClientWrapper
    {
        Task SendMailAsync(SmtpClient client, MailMessage message);
    }

    public class SmtpClientWrapper : ISmtpClientWrapper
    {
        public async Task SendMailAsync(SmtpClient client, MailMessage message)
        {
            await client.SendMailAsync(message);
        }
    }
}