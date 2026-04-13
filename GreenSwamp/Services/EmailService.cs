using GreenSwamp.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace GreenSwamp.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            try
            {
                
                var secureOptions = _mailSettings.UseSSL
                    ? SecureSocketOptions.SslOnConnect   
                    : SecureSocketOptions.StartTls;       

                Console.WriteLine($"Connecting to {_mailSettings.Host}:{_mailSettings.Port} with {secureOptions}");

                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, secureOptions);

                await smtp.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
                await smtp.SendAsync(email);

                Console.WriteLine($"✅ Email successfully sent to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send email: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}