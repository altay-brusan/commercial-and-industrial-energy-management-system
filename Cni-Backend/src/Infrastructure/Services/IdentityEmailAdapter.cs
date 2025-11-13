using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Backend.Infrastructure.Services;
public class IdentityEmailAdapter<TUser> : IEmailSender<TUser>
    where TUser : class
{
    private readonly IEmailService _emailService;

    public IdentityEmailAdapter(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
    {
        var htmlContent = $"""
            <h3>Email Doğrulama</h3>
            <p>Hesabınızı doğrulamak için <a href='{confirmationLink}'>bu linke tıklayın</a>.</p>
            """;

        return _emailService.SendEmailAsync(email, "Email Doğrulama", htmlContent);
    }

    public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
    {
        var htmlContent = $"""
            <h3>Şifre Sıfırlama</h3>
            <p>Şifrenizi sıfırlamak için <a href='{resetLink}'>bu linke tıklayın</a>.</p>
            """;

        return _emailService.SendEmailAsync(email, "Şifre Sıfırlama", htmlContent);
    }

    public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
    {
        var htmlContent = $"""
            <h3>Şifre Sıfırlama Kodu</h3>
            <p>Kodunuz: <strong>{resetCode}</strong></p>
            <p>Bu kod 15 dakika geçerlidir.</p>
            """;

        return _emailService.SendEmailAsync(email, "Şifre Sıfırlama Kodu", htmlContent);
    }
}
