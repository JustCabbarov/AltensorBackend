using System.Threading.Tasks;
using Altensorcrm.Contract.Services.Tenant;
using Microsoft.AspNetCore.Http;

namespace Altensorcrm.Api.Middlewares;

public class TenantStatusMiddleware
{
    private readonly RequestDelegate _next;

    public TenantStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var status = tenantService.TenantStatus;

            if (string.Equals(status, "Suspended", System.StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Şirkətin hesabı dondurulub. Platforma administratoru ilə əlaqə saxlayın.",
                    code = "TENANT_SUSPENDED"
                });
                return;
            }

            if (string.Equals(status, "Expired", System.StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Şirkətin abunəlik müddəti bitib.",
                    code = "TENANT_EXPIRED"
                });
                return;
            }
        }

        await _next(context);
    }
}
