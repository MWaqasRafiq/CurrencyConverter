using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Infrastructure.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            var startTime = DateTime.UtcNow;

            var correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;

            // request details
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var method = context.Request.Method;
            var endpoint = context.Request.Path;
            var clientId = context.Request.Headers["clientid"].FirstOrDefault();

            _logger.LogInformation("Incoming request: ClientId: {ClientId}, IP: {ClientIp}, Method: {Method}, Endpoint: {Endpoint}, CorrelationId: {CorrelationId}",
                clientId, clientIp, method, endpoint, correlationId);

            await _next(context);

            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            var statusCode = context.Response.StatusCode;

            _logger.LogInformation("Response: ClientId: {ClientId}, IP: {ClientIp}, Method: {Method}, Endpoint: {Endpoint}, StatusCode: {StatusCode}, ResponseTime: {ResponseTime}ms, CorrelationId: {CorrelationId}",
                clientId, clientIp, method, endpoint, statusCode, responseTime, correlationId);
        }
    }
}
