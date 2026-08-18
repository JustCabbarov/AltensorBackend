

using Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Presentation.ExceptionHandler
{
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<NotFoundExceptionHandler> _logger;

        public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if(exception is not NotFoundException notFoundException)
            {
                return false;
            }
            _logger.LogError(notFoundException, "NotFoundException occurred: {Message}", notFoundException.Message);

            return true;
        }
    }
}
