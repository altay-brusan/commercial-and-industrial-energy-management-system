using Microsoft.Extensions.Configuration;

namespace Backend.Web.Infrastructure;

public class AlmApiKeyEndpointFilter : IEndpointFilter
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly string _configuredApiKey;

    public AlmApiKeyEndpointFilter(IConfiguration configuration)
    {
        _configuredApiKey = configuration.GetValue<string>("AlmSettings:ApiKey")
            ?? throw new InvalidOperationException("AlmSettings:ApiKey is not configured.");
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey) ||
            !string.Equals(providedKey, _configuredApiKey, StringComparison.Ordinal))
        {
            return TypedResults.Json(
                new { error = new { code = "UNAUTHORIZED", message = "Invalid or missing API key" } },
                statusCode: 401);
        }

        return await next(context);
    }
}
