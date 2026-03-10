namespace Backend.Infrastructure.Configurations;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "DERMS-Backend";
    public string Audience { get; set; } = "DERMS-Frontend";
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
