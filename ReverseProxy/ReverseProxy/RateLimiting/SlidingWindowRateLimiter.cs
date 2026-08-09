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


        public IEnumerable<RateLimitRule> GetApplicableRules(HttpContext context)
        {
            var limits = _config.GetSection("RedisRateLimits").Get<RateLimitRule[]>();
            var applicableRules = limits
                .Where(x => x.MatchPath(context.Request.Path))
                .OrderBy(x => x.MaxRequests)
                .GroupBy(x => new { x.PathKey, x.WindowSeconds })
                .Select(x => x.First());
            return applicableRules;
        }

        private async Task<bool> IsLimited(IEnumerable<RateLimitRule> rules,string apikey)
        {
            var keys = rules.Select(x => new RedisKey($"{x.PathKey} : {{{apikey}}} :{x.WindowSeconds}")).ToArray();
            var args = new List<RedisValue> { rules.Count() };
            foreach(var rule in rules)
            {
                args.Add(rule.WindowSeconds);
                args.Add(rule.MaxRequests);
            }
            return (int)await _db.ScriptEvaluateAsync(SlidingRateLimiterScript, keys, args.ToArray()) == 1;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            // Read API key from header, not hardcoded
            var apikey = context.Request.Headers["X-Api-Key"].ToString();

            if (string.IsNullOrEmpty(apikey))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var applicableRules = GetApplicableRules(context).ToList();

            // No rules match this path — allow the request through
            if (!applicableRules.Any())
            {
                await _next(context);
                return;
            }

            var limited = await IsLimited(applicableRules, apikey);

            if (limited)
            {
                // Use shortest window as Retry-After value
                // meaning "wait this many seconds and your oldest request will have expired"
                var shortestWindow = applicableRules.Min(r => r.WindowSeconds);

                context.Response.StatusCode = 429;
                context.Response.Headers["Retry-After"] = shortestWindow.ToString();
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }

            await _next(context);
        }


    }
}
