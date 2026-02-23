using Microsoft.Extensions.Configuration;

namespace Backend.Web.Infrastructure;

public class ApiKeyEndpointFilter : IEndpointFilter
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly string _configuredApiKey;

    public ApiKeyEndpointFilter(IConfiguration configuration)
    {
        _configuredApiKey = configuration.GetValue<string>("IngestSettings:ApiKey")
            ?? throw new InvalidOperationException("IngestSettings:ApiKey is not configured.");
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
