using Backend.Application.Common.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Backend.Application.UnitTests.Common.Models;

public class AuthModelsTests
{
    [Test]
    public void LoginRequest_ShouldStoreProperties()
    {
        var request = new LoginRequest("test@example.com", "Password1!", true);

        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be("Password1!");
        request.RememberMe.Should().BeTrue();
    }

    [Test]
    public void LoginRequest_RememberMe_ShouldDefaultToFalse()
    {
        var request = new LoginRequest("test@example.com", "Password1!");

        request.RememberMe.Should().BeFalse();
    }

    [Test]
    public void LoginResponse_ShouldStoreAllProperties()
    {
        var userInfo = new LoginUserInfo(
            "user-1", "test@example.com", "John", "Doe",
            "Administrator", null, new List<string> { "portfolio.view" });

        var response = new LoginResponse("token123", "Bearer", 900, userInfo);

        response.AccessToken.Should().Be("token123");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(900);
        response.User.Should().BeSameAs(userInfo);
    }

    [Test]
    public void LoginUserInfo_ShouldStoreAllProperties()
    {
        var permissions = new List<string> { "portfolio.view", "dashboard.view" };
        var userInfo = new LoginUserInfo(
            "user-1", "test@example.com", "John", "Doe",
            "TenantAdmin", "https://avatar.url/img.png", permissions);

        userInfo.Id.Should().Be("user-1");
        userInfo.Email.Should().Be("test@example.com");
        userInfo.FirstName.Should().Be("John");
        userInfo.LastName.Should().Be("Doe");
        userInfo.Role.Should().Be("TenantAdmin");
        userInfo.AvatarUrl.Should().Be("https://avatar.url/img.png");
        userInfo.Permissions.Should().HaveCount(2);
    }

    [Test]
    public void RefreshTokenResponse_ShouldStoreProperties()
    {
        var response = new RefreshTokenResponse("newToken", "Bearer", 900);

        response.AccessToken.Should().Be("newToken");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(900);
    }

    [Test]
    public void AuthErrorResponse_ShouldStoreError()
    {
        var error = new AuthError("INVALID_CREDENTIALS", "Bad password");
        var response = new AuthErrorResponse(error);

        response.Error.Code.Should().Be("INVALID_CREDENTIALS");
        response.Error.Message.Should().Be("Bad password");
    }

    [Test]
    public void AuthError_ShouldSupportOptionalFields()
    {
        var error = new AuthError("RATE_LIMIT_EXCEEDED", "Too many requests",
            new List<string> { "detail1" }, 300);

        error.Code.Should().Be("RATE_LIMIT_EXCEEDED");
        error.Message.Should().Be("Too many requests");
        error.Details.Should().Contain("detail1");
        error.RetryAfter.Should().Be(300);
    }

    [Test]
    public void AuthError_OptionalFieldsShouldDefaultToNull()
    {
        var error = new AuthError("ERR", "Message");

        error.Details.Should().BeNull();
        error.RetryAfter.Should().BeNull();
    }
}
