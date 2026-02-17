using Backend.Application.Common.Interfaces;
using Backend.Infrastructure.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

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
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _emailSettings.FromName ?? _emailSettings.FromEmail,
                _emailSettings.FromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();

            // Port 465 uses implicit SSL, port 587 uses STARTTLS
            var secureSocketOptions = _emailSettings.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            if (!_emailSettings.EnableSsl)
            {
                secureSocketOptions = SecureSocketOptions.None;
            }

            await client.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.Port,
                secureSocketOptions);

            await client.AuthenticateAsync(
                _emailSettings.Username,
                _emailSettings.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Recipient}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Recipient}", to);
            throw;
        }
    }
}
