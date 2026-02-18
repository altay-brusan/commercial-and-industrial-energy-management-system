using Backend.Application.Common.Interfaces;
using Backend.Web.Services;

namespace Backend.Web.Middleware;

public class MultiTenantMiddleware() : IMiddleware
{
    public const string SelectedTenantHeader = "X-Selected-Tenant-Id";

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var multiTenantService = context.RequestServices.GetService<IMultiTenantService>();
        if (multiTenantService is MultiTenantService service)
        {
            // 1. Extract authenticated tenant from JWT "tenant_id" claim
            var tenantIdString = context.User?.FindFirst("tenant_id")?.Value;
            var parseTenantId = Guid.TryParse(tenantIdString, out Guid authenticatedTenantId);
            var tenantId = parseTenantId ? authenticatedTenantId : (Guid?)null;

            // 2. Extract super admin status from "Administrator" role
            var isAdmin = context.User?.IsInRole("Administrator") ?? false;

            // 3. If admin, read X-Selected-Tenant-Id header for tenant impersonation
            Guid? selectedTenantId = null;
            if (isAdmin && context.Request.Headers.TryGetValue(SelectedTenantHeader, out var selectedTenantHeader))
            {
                if (Guid.TryParse(selectedTenantHeader.ToString(), out Guid parsedSelectedTenant) && parsedSelectedTenant != Guid.Empty)
                {
                    selectedTenantId = parsedSelectedTenant;
                }
            }

            // 4. Initialize the service with all values
            service.Initialize(tenantId, isAdmin, selectedTenantId);
        }

        return next(context!);
    }
}
