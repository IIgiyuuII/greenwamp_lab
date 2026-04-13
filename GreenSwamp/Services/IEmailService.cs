using Microsoft.AspNetCore.Mvc;

namespace GreenSwamp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
