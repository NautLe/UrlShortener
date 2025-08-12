using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace UrlShortener.Middlewares
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, (DateTime Timestamp, int Count)> _requests = new();

        private readonly int _limit = 5; // số request tối đa
        private readonly TimeSpan _timeWindow = TimeSpan.FromSeconds(1); // trong bao nhiêu giây

        public RateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();

            // Nếu IP null thì bỏ qua rate limit
            if (string.IsNullOrEmpty(ip))
            {
                await _next(context);
                return;
            }

            var now = DateTime.UtcNow;

            _requests.AddOrUpdate(ip,
                _ => (now, 1),
                (_, entry) =>
                {
                    if (now - entry.Timestamp > _timeWindow)
                    {
                        return (now, 1);
                    }
                    else
                    {
                        return (entry.Timestamp, entry.Count + 1);
                    }
                });

            var requestInfo = _requests[ip];
            if (requestInfo.Count > _limit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many requests. Please slow down.");
                return;
            }

            await _next(context);
        }
    }
}
