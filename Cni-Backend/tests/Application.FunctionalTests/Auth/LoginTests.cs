using System.Net;
using System.Net.Http.Json;
using Backend.Application.Common.Models;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application.FunctionalTests.Auth;

using static Testing;

[TestFixture]
public class LoginTests : BaseTestFixture
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
        string[]? roles = null,
        Guid? tenantId = null)
    {
        await ExecuteWithServiceAsync<UserManager<ApplicationUser>>(async userManager =>
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Name = "Test",
                Surname = "User",
                Title = "Test Title",
                Description = "Test Description",
                TenantId = tenantId
            };

            await userManager.CreateAsync(user, password);

            if (roles != null && roles.Length > 0)
            {
                await ExecuteWithServiceAsync<RoleManager<IdentityRole>>(async roleManager =>
                {
                    foreach (var role in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                    }
                });

                await userManager.AddToRolesAsync(user, roles);
            }
        });
    }

    [Test]
    public async Task Login_WithValidCredentials_ShouldReturn200WithTokens()
    {
        var email = $"admin-{Guid.NewGuid():N}@example.com";
        await CreateTestUserAsync(email, roles: new[] { Roles.Administrator });

        var request = new LoginRequest(email, "TestPassword1!");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/Login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();
        loginResponse.TokenType.Should().Be("Bearer");
        loginResponse.ExpiresIn.Should().Be(900); // 15 minutes * 60
        loginResponse.User.Should().NotBeNull();
        loginResponse.User.Email.Should().Be(email);
        loginResponse.User.FirstName.Should().Be("Test");
        loginResponse.User.LastName.Should().Be("User");
        loginResponse.User.Role.Should().Be(Roles.Administrator);
    }

    [Test]
    public async Task Login_WithValidCredentials_ShouldSetRefreshTokenCookie()
    {
        var email = $"cookie-{Guid.NewGuid():N}@example.com";
        await CreateTestUserAsync(email, roles: new[] { Roles.User });

        var request = new LoginRequest(email, "TestPassword1!");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/Login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.StartsWith("refreshToken="));
    }

    [Test]
    public async Task Login_WithRememberMe_ShouldSetPersistentCookie()
    {
        var email = $"remember-{Guid.NewGuid():N}@example.com";
        await CreateTestUserAsync(email, roles: new[] { Roles.User });

        var request = new LoginRequest(email, "TestPassword1!", true);

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/Login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        var refreshCookie = cookies.First(c => c.StartsWith("refreshToken="));
        refreshCookie.Should().Contain("max-age=");
    }

    [Test]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        var email = $"wrongpw-{Guid.NewGuid():N}@example.com";
        await CreateTestUserAsync(email);

        var request = new LoginRequest(email, "WrongPassword1!");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/Login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("INVALID_CREDENTIALS");
    }

    [Test]
    public async Task Login_WithNonExistentUser_ShouldReturn401()
    {
        var request = new LoginRequest("nonexistent@example.com", "Password1!");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/Login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("INVALID_CREDENTIALS");
    }

    [Test]
    public async Task Login_WithEmptyEmail_ShouldReturn400()
    {
        var request = new LoginRequest("", "Password1!");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/Login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("VALIDATION_ERROR");
    }

    [Test]
    public async Task Login_WithEmptyPassword_ShouldReturn400()
    {
        var request = new LoginRequest("test@example.com", "");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/Login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("VALIDATION_ERROR");
    }

    [Test]
    public async Task Login_ShouldStoreRefreshTokenInDatabase()
    {
        var email = $"dbtoken-{Guid.NewGuid():N}@example.com";
        await CreateTestUserAsync(email, roles: new[] { Roles.User });

        var request = new LoginRequest(email, "TestPassword1!");

        await _client.PostAsJsonAsync("/api/v1/Auth/Login", request);

        var tokenCount = await CountAsync<RefreshToken>();
        tokenCount.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Login_ShouldReturnUserPermissionsInResponse()
    {
        var email = $"perms-{Guid.NewGuid():N}@example.com";
        await CreateTestUserAsync(email, roles: new[] { Roles.Administrator });

        var request = new LoginRequest(email, "TestPassword1!");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/Login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        loginResponse.Should().NotBeNull();
        loginResponse!.User.Permissions.Should().NotBeNull();
        loginResponse.User.Permissions.Should().BeOfType<List<string>>();
    }

    [Test]
    public async Task Login_AccessTokenShouldBeDecodableJwt()
    {
        var email = $"jwt-{Guid.NewGuid():N}@example.com";
        await CreateTestUserAsync(email, roles: new[] { Roles.User });

        var request = new LoginRequest(email, "TestPassword1!");

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/Login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        loginResponse.Should().NotBeNull();

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(loginResponse!.AccessToken);

        jwt.Should().NotBeNull();
        jwt.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        jwt.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Email);
    }
}
