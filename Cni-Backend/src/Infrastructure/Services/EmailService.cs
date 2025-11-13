using Backend.Application.Common.Interfaces;
using Backend.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var smtp = new SmtpClient(_emailSettings.SmtpServer)
            {
                Port = _emailSettings.Port,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = _emailSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await smtp.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email sent to {to}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            throw;
        }
    }
}
