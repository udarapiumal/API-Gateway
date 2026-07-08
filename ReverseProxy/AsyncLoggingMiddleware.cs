using StackExchange.Redis;
using System.Diagnostics;

namespace ReverseProxy
{
    public class AsyncLoggingMiddleware
    {
        private readonly StackExchange.Redis.IDatabase _db;
        private readonly RequestDelegate _nextMiddlware;

        public AsyncLoggingMiddleware(IConnectionMultiplexer muxer , RequestDelegate nextMiddleware)
        {
            _db = muxer.GetDatabase();
            _nextMiddlware = nextMiddleware;

        }

        const string streamName = "gateway:logs";


        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew(); 

            await _nextMiddlware(context); 

            stopwatch.Stop(); 

            await _db.StreamAddAsync(streamName, new NameValueEntry[] 
            {
                new("apikey",     context.Request.Headers["X-Api-Key"].ToString()),
                new("path",       context.Request.Path.ToString()),
                new("method",     context.Request.Method),
                new("statuscode", context.Response.StatusCode.ToString()), 
                new("ms",         stopwatch.ElapsedMilliseconds.ToString()) 
            });
        }



    }
}
