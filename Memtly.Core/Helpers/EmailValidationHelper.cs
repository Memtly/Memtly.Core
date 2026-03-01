using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Memtly.Core.Helpers
{
    public class EmailValidationHelper
    {
        public static bool IsValid(string? email)
        {
            if (!string.IsNullOrWhiteSpace(email) && email.Length < 100)
            {
                try
                {
                    var mail = new MailAddress(email);
                    return Regex.IsMatch(mail.Address, @"^.+?\@.+?\..+?$", RegexOptions.Compiled);
                }
                catch { }
            }
                
            return false;
        }
    }
}