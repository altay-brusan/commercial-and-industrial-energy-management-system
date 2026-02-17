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
public class RefreshTokenTests : BaseTestFixture
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
        email ??= $"rt-{Guid.NewGuid():N}@example.com";

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
    public async Task RefreshToken_WithValidTokens_ShouldReturn200()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, refreshCookie) = await LoginAsync(email);

        refreshCookie.Should().NotBeNull();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Cookie", refreshCookie);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(900);
    }

    [Test]
    public async Task RefreshToken_ShouldReturnNewRefreshTokenCookie()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, refreshCookie) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Cookie", refreshCookie!);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.StartsWith("refreshToken="));
    }

    [Test]
    public async Task RefreshToken_ShouldReturnDifferentAccessToken()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, refreshCookie) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Cookie", refreshCookie!);

        var response = await _client.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        result!.AccessToken.Should().NotBe(accessToken);
    }

    [Test]
    public async Task RefreshToken_WithoutRefreshCookie_ShouldReturn401()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, _) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("INVALID_REFRESH_TOKEN");
    }

    [Test]
    public async Task RefreshToken_WithoutAuthorizationHeader_ShouldReturn401()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (_, refreshCookie) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request.Headers.Add("Cookie", refreshCookie!);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("INVALID_TOKEN");
    }

    [Test]
    public async Task RefreshToken_WithInvalidAccessToken_ShouldReturn401()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (_, refreshCookie) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-jwt-token");
        request.Headers.Add("Cookie", refreshCookie!);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task RefreshToken_WithFakeRefreshCookie_ShouldReturn401()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, _) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Cookie", "refreshToken=fake-refresh-token-value");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("INVALID_REFRESH_TOKEN");
    }

    [Test]
    public async Task RefreshToken_UsedTokenShouldBeRevoked()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, refreshCookie) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Cookie", refreshCookie!);

        await _client.SendAsync(request);

        var revokedTokens = await GetAllAsync<RefreshToken>(rt => rt.IsRevoked);
        revokedTokens.Should().NotBeEmpty();
    }

    [Test]
    public async Task RefreshToken_ShouldCreateTokenRotationChain()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, refreshCookie) = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Cookie", refreshCookie!);

        await _client.SendAsync(request);

        var revokedToken = await FirstOrDefaultAsync<RefreshToken>(rt => rt.IsRevoked);
        revokedToken.Should().NotBeNull();
        revokedToken!.ReplacedByToken.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task RefreshToken_ReusingOldToken_ShouldReturn401()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var (accessToken, refreshCookie) = await LoginAsync(email);

        // First refresh — should succeed
        var request1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request1.Headers.Add("Cookie", refreshCookie!);

        var response1 = await _client.SendAsync(request1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        var newResult = await response1.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        // Second refresh with same old cookie — should fail (token already revoked)
        var request2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/RefreshToken");
        request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newResult!.AccessToken);
        request2.Headers.Add("Cookie", refreshCookie!);

        var response2 = await _client.SendAsync(request2);

        response2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
