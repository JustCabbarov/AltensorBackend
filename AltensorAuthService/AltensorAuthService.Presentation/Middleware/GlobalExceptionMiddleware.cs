using AltensorAuthService.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace AltensorAuthService.Presentation.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, title, detail) = exception switch
            {
                ValidationException valEx => (
                    (int)HttpStatusCode.BadRequest,
                    "Validasiya Xətası",
                    valEx.Message
                ),
                UnauthorizedException unAuthEx => (
                    (int)HttpStatusCode.Unauthorized,
                    "İcazəsiz Giriş",
                    unAuthEx.Message
                ),
                ForbiddenException forbEx => (
                    (int)HttpStatusCode.Forbidden,
                    "Qadağan Olunmuş Əməliyyat",
                    forbEx.Message
                ),
                TenantSuspendedException suspEx => (
                    (int)HttpStatusCode.Forbidden,
                    "Hesab Dondurulub",
                    suspEx.Message
                ),
                NotFoundException notFoundEx => (
                    (int)HttpStatusCode.NotFound,
                    "Resurs Tapılmadı",
                    notFoundEx.Message
                ),
                KeyNotFoundException keyEx => (
                    (int)HttpStatusCode.NotFound,
                    "Resurs Tapılmadı",
                    keyEx.Message
                ),
                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    "Daxili Server Xətası",
                    "Gözlənilməz bir xəta baş verdi. Zəhmət olmasa bir az sonra yenidən cəhd edin."
                )
            };

            context.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
