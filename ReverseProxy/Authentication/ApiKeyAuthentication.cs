using Microsoft.AspNetCore.Http.HttpResults;

namespace ReverseProxy.Authentication
{
    public class ApiKeyAuthentication
    {
        private readonly RequestDelegate _nextMiddleware;
        private const string ApiKeyheaderName = "X-Api-Key";
        private readonly IConfiguration _configuration;

        public ApiKeyAuthentication(RequestDelegate nextMiddleware,IConfiguration configuration)
        {
            _nextMiddleware = nextMiddleware;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            string apikey = context.Request.Headers[ApiKeyheaderName];

            if (!ApiKeyValid(apikey))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _nextMiddleware(context);
        }

        public bool ApiKeyValid(string? apikey)
        {
            string ActualApiKey = _configuration.GetValue<string>("ApiKey");
            Console.WriteLine("apikeymy"+ActualApiKey);

            if(apikey == ActualApiKey)
            {
                return true;
            }
            else
            {
                return false;
            }
           
        }

    }
}
