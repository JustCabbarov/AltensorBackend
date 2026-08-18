using Microsoft.AspNetCore.Diagnostics;

namespace Presentation.ExceptionHandler
{
    public class UnauthorizedExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<UnauthorizedExceptionHandler> _logger;

        public UnauthorizedExceptionHandler(ILogger<UnauthorizedExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not UnauthorizedAccessException unauthorizedAccessException)
            {
                return false;
            }

            _logger.LogError(unauthorizedAccessException, "UnauthorizedAccessException occurred: {Message}", unauthorizedAccessException.Message);

            return true;
        }
    }
}
