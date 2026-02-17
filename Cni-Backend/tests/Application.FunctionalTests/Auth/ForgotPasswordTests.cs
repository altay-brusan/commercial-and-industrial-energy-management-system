using System.Net;
using System.Net.Http.Json;
using Backend.Application.Common.Models;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.FunctionalTests.Auth;

using static Testing;

[TestFixture]
public class ForgotPasswordTests : BaseTestFixture
{
    private HttpClient _client = null!;

    [SetUp]
    public void SetUpClient()
    {
        _client = CreateClient();
    }

    [TearDown]
    public void TearDownClient()
    {
        _client.Dispose();
    }

    private async Task CreateTestUserAsync(
        string email,
        string password = "TestPassword1!",
        bool emailConfirmed = true)
    {
        await ExecuteWithServiceAsync<UserManager<ApplicationUser>>(async userManager =>
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = emailConfirmed,
                Name = "Test",
                Surname = "User",
                Title = "Test Title",
                Description = "Test Description"
            };

            await userManager.CreateAsync(user, password);
        });
    }

    [Test]
    public async Task ForgotPassword_WithExistingUser_ShouldReturn200()
    {
        var email = $"forgot-{Guid.NewGuid():N}@example.com";
        await CreateTestUserAsync(email);

        var request = new ForgotPasswordRequest(email);

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/ForgotPassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task ForgotPassword_WithNonExistentEmail_ShouldReturn200()
    {
        var request = new ForgotPasswordRequest("nonexistent@example.com");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/ForgotPassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task ForgotPassword_ShouldReturnCorrectMessage()
    {
        var request = new ForgotPasswordRequest("any@example.com");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/ForgotPassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Be("If an account with this email exists, a password reset link has been sent.");
    }

    [Test]
    public async Task ForgotPassword_WithEmptyEmail_ShouldReturn400()
    {
        var request = new ForgotPasswordRequest("");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/ForgotPassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("VALIDATION_ERROR");
    }

    [Test]
    public async Task ForgotPassword_WithInvalidEmailFormat_ShouldReturn200()
    {
        // Invalid format still returns 200 to prevent email enumeration
        // The user simply won't be found, so no email is sent
        var request = new ForgotPasswordRequest("not-an-email");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/ForgotPassword", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Be("If an account with this email exists, a password reset link has been sent.");
    }
}
