using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CurrencyConverter.Infrastructure.Middleware
{
    public class ValidateClientMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        
        public ValidateClientMiddleWare(RequestDelegate next,IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
         }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = context.Request.Headers["clientid"].FirstOrDefault();

            if (string.IsNullOrEmpty(clientId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("ClientId is missing.");
                return;
            }

            var allowedClientIds = _configuration.GetSection("clientid").Get<string[]>();

            if (allowedClientIds == null || !allowedClientIds.Contains(clientId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid Client.");
                return;
            }
            await _next(context);
        }
    }
}
