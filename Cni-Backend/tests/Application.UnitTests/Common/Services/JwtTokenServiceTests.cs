using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Infrastructure.Configurations;
using Backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;

namespace Backend.Application.UnitTests.Common.Services;

public class JwtTokenServiceTests
{
    private JwtTokenService _sut = null!;
    private JwtSettings _jwtSettings = null!;

    [SetUp]
    public void Setup()
    {
        _jwtSettings = new JwtSettings
        {
            Secret = "TestSuperSecretKeyThatIsAtLeast32BytesLong!!2024",
            Issuer = "DERMS-Backend-Test",
            Audience = "DERMS-Frontend-Test",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        var options = Mock.Of<IOptions<JwtSettings>>(o => o.Value == _jwtSettings);
        _sut = new JwtTokenService(options);
    }

    [Test]
    public void GenerateAccessToken_ShouldReturnValidJwt()
    {
        var token = _sut.GenerateAccessToken(
            "user-123", "test@example.com", "1",
            new List<string> { "Administrator" },
            new List<string> { "portfolio.view" });

        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("DERMS-Backend-Test");
        jwt.Audiences.Should().Contain("DERMS-Frontend-Test");
    }

    [Test]
    public void GenerateAccessToken_ShouldIncludeUserIdClaim()
    {
        var token = _sut.GenerateAccessToken(
            "user-123", "test@example.com", "1",
            new List<string> { "Administrator" },
            new List<string>());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == "user-123");
    }

    [Test]
    public void GenerateAccessToken_ShouldIncludeEmailClaim()
    {
        var token = _sut.GenerateAccessToken(
            "user-123", "test@example.com", "1",
            new List<string> { "User" },
            new List<string>());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Email && c.Value == "test@example.com");
    }

    [Test]
    public void GenerateAccessToken_ShouldIncludeTenantIdClaim()
    {
        var token = _sut.GenerateAccessToken(
            "user-123", "test@example.com", "42",
            new List<string> { "User" },
            new List<string>());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == "tenant_id" && c.Value == "42");
    }

    [Test]
    public void GenerateAccessToken_ShouldOmitTenantIdClaimWhenNull()
    {
        var token = _sut.GenerateAccessToken(
            "user-123", "test@example.com", null,
            new List<string> { "Administrator" },
            new List<string>());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().NotContain(c => c.Type == "tenant_id");
    }

    [Test]
    public void GenerateAccessToken_ShouldIncludeRoleClaims()
    {
        var roles = new List<string> { "Administrator", "TenantAdmin" };

        var token = _sut.GenerateAccessToken(
            "user-123", "test@example.com", "1",
            roles, new List<string>());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        roleClaims.Should().Contain("Administrator");
        roleClaims.Should().Contain("TenantAdmin");
    }

    [Test]
    public void GenerateAccessToken_ShouldIncludePermissionClaims()
    {
        var permissions = new List<string> { "portfolio.view", "dashboard.view" };

        var token = _sut.GenerateAccessToken(
            "user-123", "test@example.com", "1",
            new List<string> { "User" }, permissions);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var permClaims = jwt.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
        permClaims.Should().Contain("portfolio.view");
        permClaims.Should().Contain("dashboard.view");
    }

    [Test]
    public void GenerateAccessToken_ShouldSetCorrectExpiry()
    {
        var token = _sut.GenerateAccessToken(
            "user-123", "test@example.com", "1",
            new List<string> { "User" },
            new List<string>());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(30));
    }

    [Test]
    public void GenerateAccessToken_ShouldIncludeJtiClaim()
    {
        var token = _sut.GenerateAccessToken(
            "user-123", "test@example.com", "1",
            new List<string> { "User" },
            new List<string>());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        var jti = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        Guid.TryParse(jti, out _).Should().BeTrue();
    }

    [Test]
    public void GenerateAccessToken_ShouldProduceTokenValidatableByMatchingKey()
    {
        var token = _sut.GenerateAccessToken(
            "user-123", "test@example.com", "1",
            new List<string> { "User" },
            new List<string>());

        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        }, out var validatedToken);

        principal.Should().NotBeNull();
        validatedToken.Should().BeOfType<JwtSecurityToken>();
    }

    [Test]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        var refreshToken = _sut.GenerateRefreshToken();

        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        var refreshToken = _sut.GenerateRefreshToken();

        var act = () => Convert.FromBase64String(refreshToken);
        act.Should().NotThrow();
    }

    [Test]
    public void GenerateRefreshToken_ShouldReturn64BytesWorthOfData()
    {
        var refreshToken = _sut.GenerateRefreshToken();

        var bytes = Convert.FromBase64String(refreshToken);
        bytes.Length.Should().Be(64);
    }

    [Test]
    public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
    {
        var tokens = Enumerable.Range(0, 100).Select(_ => _sut.GenerateRefreshToken()).ToList();

        tokens.Distinct().Count().Should().Be(100);
    }

    [Test]
    public void GetPrincipalFromExpiredToken_ShouldReturnPrincipalForExpiredToken()
    {
        // Create a token with very short expiry that's already expired
        var expiredSettings = new JwtSettings
        {
            Secret = _jwtSettings.Secret,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            AccessTokenExpirationMinutes = 0, // expires immediately
            RefreshTokenExpirationDays = 7
        };
        var expiredOptions = Mock.Of<IOptions<JwtSettings>>(o => o.Value == expiredSettings);
        var expiredService = new JwtTokenService(expiredOptions);

        // Generate a token that is essentially expired (0 minutes)
        var token = expiredService.GenerateAccessToken(
            "user-456", "expired@example.com", "1",
            new List<string> { "User" },
            new List<string>());

        // Use the main service to validate (it will ignore lifetime)
        var principal = _sut.GetPrincipalFromExpiredToken(token);

        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("user-456");
    }

    [Test]
    public void GetPrincipalFromExpiredToken_ShouldReturnPrincipalForValidToken()
    {
        var token = _sut.GenerateAccessToken(
            "user-789", "valid@example.com", "2",
            new List<string> { "TenantAdmin" },
            new List<string> { "portfolio.view" });

        var principal = _sut.GetPrincipalFromExpiredToken(token);

        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("user-789");
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be("valid@example.com");
        principal.FindFirst("tenant_id")?.Value.Should().Be("2");
    }

    [Test]
    public void GetPrincipalFromExpiredToken_ShouldReturnNullForInvalidToken()
    {
        var principal = _sut.GetPrincipalFromExpiredToken("not-a-valid-jwt-token");

        principal.Should().BeNull();
    }

    [Test]
    public void GetPrincipalFromExpiredToken_ShouldReturnNullForTokenWithWrongSigningKey()
    {
        var wrongSettings = new JwtSettings
        {
            Secret = "ADifferentSecretKeyThatIsAlsoAtLeast32Bytes!!",
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        var wrongOptions = Mock.Of<IOptions<JwtSettings>>(o => o.Value == wrongSettings);
        var wrongService = new JwtTokenService(wrongOptions);

        var token = wrongService.GenerateAccessToken(
            "user-123", "test@example.com", "1",
            new List<string> { "User" },
            new List<string>());

        var principal = _sut.GetPrincipalFromExpiredToken(token);

        principal.Should().BeNull();
    }
}
