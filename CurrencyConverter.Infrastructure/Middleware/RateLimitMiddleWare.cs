using System.Runtime.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CurrencyConverter.Infrastructure.Middleware
{
    public class RateLimitMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly MemoryCache _cache = new MemoryCache("RateLimit");
        private readonly int Limit = 15;
        private readonly IConfiguration _configuration;
        
        public RateLimitMiddleWare(RequestDelegate next,IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
            Limit = Convert.ToInt32(_configuration["ApiThrottling:RateLimit"]);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            int requestCount = 1;
            if (!string.IsNullOrEmpty(ip) && ip!= "::1")
            {
                var cacheKey = $"RateLimit:{ip}";
                if(_cache.GetCacheItem(cacheKey)!=null)
                    requestCount = Convert.ToInt32(_cache.GetCacheItem(cacheKey).Value);
                if (requestCount >= Limit)
                {

                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                    return;
                }

                _cache.Set(cacheKey, requestCount + 1, DateTimeOffset.Now.AddSeconds(Convert.ToInt32(_configuration["ApiThrottling:TimeWindowInSeconds"])));
            }
            await _next(context);
        }
    }
}
