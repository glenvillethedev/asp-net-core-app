using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ExpensesTracker.Middlewares
{
    /// <summary>
    /// Middleware for logging requests with custom Correlation ID.
    /// </summary>
    public class RequestIdLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger<RequestIdLoggingMiddleware> _logger;

        public RequestIdLoggingMiddleware(RequestDelegate next, ILogger<RequestIdLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            using(_logger.BeginScope("CorrelationID: {CorrelationID}", Guid.NewGuid()))
            {
                _logger.LogInformation("{RequestMethod} {RequestPath}, Request received.", httpContext.Request.Method, httpContext.Request.Path);

                await _next(httpContext);

                _logger.LogInformation("{RequestMethod} {RequestPath}, Request processing completed.", httpContext.Request.Method, httpContext.Request.Path);
            }            
        }
    }

    public static class RequestIdLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestIdLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestIdLoggingMiddleware>();
        }
    }
}
