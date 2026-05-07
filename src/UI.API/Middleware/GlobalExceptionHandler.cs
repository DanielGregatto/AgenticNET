using Domain.Contracts.API;
using Microsoft.AspNetCore.Diagnostics;

namespace UI.API.Middleware
{
    /// <summary>
    /// Global exception handler that catches all unhandled exceptions, logs them with correlation ID,
    /// and returns a consistent error response matching the application's Result pattern.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Get correlation ID from response headers (set by CorrelationIdMiddleware) or fall back to TraceIdentifier
            var correlationId = httpContext.Response.Headers[CorrelationIdHeader].FirstOrDefault()
                ?? httpContext.TraceIdentifier;

            // Log the exception with correlation ID (automatically prepended by CorrelationLogger)
            _logger.LogError(
                exception,
                "Unhandled exception occurred. Path: {Path}, Method: {Method}, User: {User}",
                httpContext.Request.Path,
                httpContext.Request.Method,
                httpContext.User?.Identity?.Name ?? "Anonymous"
            );

            // Create error response matching the application's error format
            var errorResponse = new ErrorResponseDto(
                type: "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title: "An error occurred while processing your request",
                status: 500,
                traceId: correlationId, // Use correlation ID instead of new GUID
                items: new List<ErrorResponseItemDto>
                {
                    new ErrorResponseItemDto(
                        type: "UnhandledException",
                        errorDesc: exception.Message
                    )
                }
            );

            // Set response properties
            httpContext.Response.StatusCode = 500;
            httpContext.Response.ContentType = "application/json";

            // Write the error response
            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            // Return true to indicate the exception has been handled
            return true;
        }
    }
}
