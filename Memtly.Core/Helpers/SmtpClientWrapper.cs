using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Memtly.Core.Helpers
{
    public interface ISmtpClientWrapper
    {
        Task ConnectAsync(SmtpClient client, string host, int port, SecureSocketOptions options = SecureSocketOptions.StartTls);
        Task AuthenticateAsync(SmtpClient client, NetworkCredential creds);
        Task SendAsync(SmtpClient client, MimeMessage message);
        Task DisconnectAsync(SmtpClient client, bool quit = true);
    }

    public class SmtpClientWrapper : ISmtpClientWrapper
    {
        public async Task ConnectAsync(SmtpClient client, string host, int port, SecureSocketOptions options = SecureSocketOptions.StartTls)
        {
            await client.ConnectAsync(host, port, options);
        }

        public async Task AuthenticateAsync(SmtpClient client, NetworkCredential creds)
        {
            await client.AuthenticateAsync(creds.UserName, creds.Password);
        }

        public async Task SendAsync(SmtpClient client, MimeMessage message)
        {
            await client.SendAsync(message);
        }

        public async Task DisconnectAsync(SmtpClient client, bool quit = true)
        {
            await client.DisconnectAsync(quit);
        }
    }
}