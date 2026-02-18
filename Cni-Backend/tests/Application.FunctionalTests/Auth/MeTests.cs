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
public class MeTests : BaseTestFixture
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
        string[]? roles = null,
        Guid? tenantId = null)
    {
        email ??= $"me-{Guid.NewGuid():N}@example.com";

        await ExecuteWithServiceAsync<UserManager<ApplicationUser>>(async userManager =>
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Name = "John",
                Surname = "Doe",
                Title = "Engineer",
                Description = "Test user",
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

        return email;
    }

    private async Task<string> LoginAndGetTokenAsync(
        string email,
        string password = "TestPassword1!")
    {
        var loginRequest = new LoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/Auth/Login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResult!.AccessToken;
    }

    [Test]
    public async Task Me_WithValidToken_ShouldReturn200WithUserProfile()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var accessToken = await LoginAndGetTokenAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Auth/Me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MeResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(email);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Role.Should().Be(Roles.User);
        result.Permissions.Should().NotBeNull();
    }

    [Test]
    public async Task Me_ShouldReturnCorrectRole()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.Administrator });
        var accessToken = await LoginAndGetTokenAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Auth/Me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<MeResponse>();

        result!.Role.Should().Be(Roles.Administrator);
    }

    [Test]
    public async Task Me_ShouldReturnPermissionsList()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var accessToken = await LoginAndGetTokenAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Auth/Me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<MeResponse>();

        result!.Permissions.Should().NotBeNull();
        result.Permissions.Should().BeOfType<List<string>>();
    }

    [Test]
    public async Task Me_WithoutTenant_ShouldReturnNullTenantFields()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.User });
        var accessToken = await LoginAndGetTokenAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Auth/Me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<MeResponse>();

        result!.TenantId.Should().BeNull();
        result.TenantName.Should().BeNull();
    }

    [Test]
    public async Task Me_WithoutAuthorizationHeader_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Auth/Me");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("UNAUTHORIZED");
        error.Error.Message.Should().Be("Access token is missing or invalid");
    }

    [Test]
    public async Task Me_WithInvalidToken_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Auth/Me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-jwt-token");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<AuthErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("UNAUTHORIZED");
    }

    [Test]
    public async Task Me_ShouldReturnSameEmailAsLogin()
    {
        var email = await CreateTestUserAsync(roles: new[] { Roles.Operator });
        var accessToken = await LoginAndGetTokenAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Auth/Me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<MeResponse>();

        result!.Email.Should().Be(email);
        result.Role.Should().Be(Roles.Operator);
    }
}
