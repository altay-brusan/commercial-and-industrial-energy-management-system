using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Backend.Infrastructure.Logging
{
    public class UserEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            var userName = httpContext?.User?.Identity?.Name ?? "Anonymous";

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", userName));
        }
    }
}
