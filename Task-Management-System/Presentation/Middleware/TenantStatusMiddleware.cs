using Contract.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Presentation.Middleware
{
    /// <summary>
    /// Bütün authenticate olmuş sorğularda tenant statusunu yoxlayır.
    /// Suspended/Expired tenantlar 403 alır.
    /// Middleware sırası: UseAuthentication → UseAuthorization → bu middleware → MapControllers
    /// </summary>
    public class TenantStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantStatusMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var status = tenantService.TenantStatus;

                if (status == "Suspended")
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Şirkətin hesabı dondurulub. Platforma administratoru ilə əlaqə saxlayın.",
                        code  = "TENANT_SUSPENDED"
                    });
                    return;
                }

                if (status == "Expired")
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Şirkətin abunəlik müddəti bitib. Platforma administratoru ilə əlaqə saxlayın.",
                        code  = "TENANT_EXPIRED"
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
}
