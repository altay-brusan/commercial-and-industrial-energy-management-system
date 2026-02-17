using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Backend.Application.Common.Models;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application.FunctionalTests.Auth;

using static Testing;

[TestFixture]
public class LogoutTests : BaseTestFixture
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

    private async Task<string> CreateTestUserAsync(
        string? email = null,
        string password = "TestPassword1!",
        string[]? roles = null)
    {
        email ??= $"logout-{Guid.NewGuid():N}@example.com";

        await ExecuteWithServiceAsync<UserManager<ApplicationUser>>(async userManager =>
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Name = "Test",
                Surname = "User",
                Title = "Test Title",
                Description = "Test Description"
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

        return email;
    }

    private async Task<(string accessToken, string? refreshCookie)> LoginAsync(
        string email,
        string password = "TestPassword1!")
    {
        var loginRequest = new LoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/Auth/Login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        string? refreshCookie = null;
        if (loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var cookieValue = cookies.FirstOrDefault(c => c.StartsWith("refreshToken="));
            if (cookieValue != null)
            {
                refreshCookie = cookieValue.Split(';')[0]; // "refreshToken=<value>"
            }
        }

        return (loginResult!.AccessToken, refreshCookie);
    }

    [Test]
    public async Task Logout_WithValidToken_ShouldReturn200()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, _) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/Logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LogoutResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Be("Successfully logged out");
    }

    [Test]
    public async Task Logout_ShouldClearRefreshTokenCookie()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, _) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/Logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        var refreshCookie = cookies.First(c => c.StartsWith("refreshToken="));
        refreshCookie.Should().Contain("max-age=0");
    }

    [Test]
    public async Task Logout_ShouldRevokeRefreshTokenInDatabase()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, refreshCookie) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/Logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Cookie", refreshCookie!);

        await _client.SendAsync(request);

        var activeTokens = await GetAllAsync<RefreshToken>(rt => !rt.IsRevoked);
        // The token created during login should now be revoked
        var revokedTokens = await GetAllAsync<RefreshToken>(rt => rt.IsRevoked);
        revokedTokens.Should().NotBeEmpty();
    }

    [Test]
    public async Task Logout_RefreshTokenShouldNotWorkAfterLogout()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, refreshCookie) = await LoginAsync(email);

        // Logout
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/Logout");
        logoutRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        logoutRequest.Headers.Add("Cookie", refreshCookie!);

        var logoutResponse = await _client.SendAsync(logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Try to use the old refresh token
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        refreshRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        refreshRequest.Headers.Add("Cookie", refreshCookie!);

        var refreshResponse = await _client.SendAsync(refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Logout_WithoutAuthorizationHeader_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/Logout");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("UNAUTHORIZED");
        error.Error.Message.Should().Be("Access token is missing or invalid");
    }

    [Test]
    public async Task Logout_WithInvalidToken_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/Logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-jwt-token");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("UNAUTHORIZED");
    }

    [Test]
    public async Task Logout_WithoutRefreshCookie_ShouldStillReturn200()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, _) = await LoginAsync(email);

        // Logout without sending the refresh cookie
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/Logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        // Should still succeed — clears cookie and returns 200
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LogoutResponse>();
        result!.Message.Should().Be("Successfully logged out");
    }
}
