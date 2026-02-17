using Backend.Infrastructure.Configurations;
using FluentAssertions;
using NUnit.Framework;

namespace Backend.Application.UnitTests.Common.Configurations;

public class JwtSettingsTests
{
    [Test]
    public void DefaultValues_ShouldHaveCorrectDefaults()
    {
        var settings = new JwtSettings();

        settings.Secret.Should().BeEmpty();
        settings.Issuer.Should().Be("DERMS-Backend");
        settings.Audience.Should().Be("DERMS-Frontend");
        settings.AccessTokenExpirationMinutes.Should().Be(15);
        settings.RefreshTokenExpirationDays.Should().Be(7);
    }

    [Test]
    public void Properties_ShouldBeSettable()
    {
        var settings = new JwtSettings
        {
            Secret = "my-secret-key",
            Issuer = "custom-issuer",
            Audience = "custom-audience",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 14
        };

        settings.Secret.Should().Be("my-secret-key");
        settings.Issuer.Should().Be("custom-issuer");
        settings.Audience.Should().Be("custom-audience");
        settings.AccessTokenExpirationMinutes.Should().Be(30);
        settings.RefreshTokenExpirationDays.Should().Be(14);
    }
}
