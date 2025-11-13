using Backend.Application.Common.Interfaces;
using Backend.Web.Endpoints;
using Backend.Web.Services;

namespace Backend.Web.Middleware;

public class MultiTenantMiddleware() : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var multiTenantService = context.RequestServices.GetService<IMultiTenantService>();
        if (multiTenantService != null)
        {
            var tenantIDString = context?.User?.FindFirst("tenant_id")?.Value;
            var isadmin = context?.User?.IsInRole("Administrator");
            multiTenantService.SetSuperAdmin(isadmin!=null?isadmin.Value:false);

            var parseTenantId = int.TryParse(tenantIDString, out int tenantId);
            multiTenantService.SetCurrentTenantId(parseTenantId ? tenantId : null);
        }
        return next(context!);
    }
}
