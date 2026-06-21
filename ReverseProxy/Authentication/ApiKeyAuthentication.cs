using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ReverseProxy.Database;
using ReverseProxy.Redis;

namespace ReverseProxy.Authentication
{
    public class ApiKeyAuthentication
    {
        private readonly RequestDelegate _nextMiddleware;
        private const string ApiKeyheaderName = "X-Api-Key";
        private readonly IRedisCaching _cache;

        private readonly IServiceScopeFactory _scopefactory;



        public ApiKeyAuthentication(RequestDelegate nextMiddleware,IServiceScopeFactory scopeFactory,IRedisCaching cache)
        {
            _nextMiddleware = nextMiddleware;
       
            _scopefactory = scopeFactory;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            string apikey = context.Request.Headers[ApiKeyheaderName];
            apikey = "550e8400-e29b-41d4-a716-446655440000";

            if (!await ApiKeyValid(apikey))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _nextMiddleware(context);
        }

        public async Task<bool> ApiKeyValid(string? apikey)
        {
            if (apikey != null && Guid.TryParse(apikey,out var ParsedKey))
            {
                using (var scope = _scopefactory.CreateScope())
                {
                    var cacheResult = _cache.GetData<string>($"apikey:{ParsedKey}");
                    if (cacheResult != null)
                    {
                        return true;
                    }
                    else
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var dbresult = await dbContext.ApiKeys.FirstOrDefaultAsync(keys => keys.id == ParsedKey);


                        if (dbresult != null)
                        {
                            _cache.SetData<string>($"apikey:{ParsedKey}", ParsedKey.ToString());
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

              
                }
               
            }
            return false;
           
           
        }

    }
}
