using Application.Exceptions;

using Microsoft.AspNetCore.Diagnostics;

namespace Presentation.ExceptionHandler
{
    public class NullExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<NullExceptionHandler> _logger;

        public NullExceptionHandler(ILogger<NullExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
           if (exception is not NotNullExceptions argumentNullException)
           {
               return  false;
            }
              _logger.LogError(argumentNullException, "ArgumentNullException occurred: {Message}", argumentNullException.Message);
                return true;
        }
    }
}
