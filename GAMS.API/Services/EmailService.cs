using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;
namespace GAMS.API.Services
{
    public class EmailService : IEmailService
    {
        private const string SmtpHost = "sandbox.smtp.mailtrap.io";
        private const int SmtpPort = 587;
        private const string SmtpUser = "e13da1222fc102";
        private const string SmtpPass = "****e3e4";

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("GAMS", "hello@demomailtrap.co"));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(SmtpHost, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(SmtpUser, SmtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}