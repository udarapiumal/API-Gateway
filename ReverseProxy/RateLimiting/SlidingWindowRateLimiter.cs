using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System.Threading.RateLimiting;

namespace ReverseProxy.RateLimiting
{
    public static class SlidingWindowRateLimiterExtensions
    {
        public static void UseSlidingWindowRateLimiter(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<SlidingWindowRateLimiter>();
        }
    }

    public class SlidingWindowRateLimiter
    {
        private const string SlidingRateLimiterScript = @"

        local current_time = redis.call('TIME')
        local num_windows = ARGV[1]

        for i=2, num_windows*2, 2 do
            local window = ARGV[i]
            local max_requests = ARGV[i+1]
            local curr_key = KEYS[i/2]
            local trim_time = tonumber(current_time[1]) - window
            redis.call('ZREMRANGEBYSCORE', curr_key, 0, trim_time)
            local request_count = redis.call('ZCARD',curr_key)
            if request_count >= tonumber(max_requests) then
                return 1
            end
        end

        for i=2, num_windows*2, 2 do
            local curr_key = KEYS[i/2]
            local window = ARGV[i]
            redis.call('ZADD', curr_key, current_time[1], current_time[1] .. current_time[2])
            redis.call('EXPIRE', curr_key, window)
        end
        return 0


        ";
        private readonly StackExchange.Redis.IDatabase _db;
        private readonly IConfiguration _config;
        private readonly RequestDelegate _next;


        public SlidingWindowRateLimiter(RequestDelegate next , IConnectionMultiplexer muxer , IConfiguration config)
        {
            _db = muxer.GetDatabase();
            _config = config;
            _next = next;
        }
    }
}
