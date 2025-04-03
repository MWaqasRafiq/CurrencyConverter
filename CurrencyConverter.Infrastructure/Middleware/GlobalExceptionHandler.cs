using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace CurrencyExchange.Infrastructure.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;
        public GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
            _environment = env;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError($"An error occurred while processing your request: {exception.Message}");
            var response = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.",
                StackTrace = _environment.IsDevelopment() ? exception.StackTrace : ""
            };
            var jsonResponse = JsonSerializer.Serialize(response);
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);
            return true;
        }
    }
}