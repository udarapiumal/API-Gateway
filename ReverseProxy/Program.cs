using Microsoft.EntityFrameworkCore;
using ReverseProxy;
using ReverseProxy.Authentication;
using ReverseProxy.Database;
using ReverseProxy.RateLimiting;
using ReverseProxy.Redis;
using StackExchange.Redis;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IRedisCaching, RedisService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"))
);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "cache";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Your React URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHostedService<ConsumerService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseCors("AllowFrontend");

app.UseMiddleware<AsyncLoggingMiddleware>();
app.UseSlidingWindowRateLimiter();

app.UseMiddleware<ApiKeyAuthentication>();
app.UseMiddleware<ReverseProxyMiddleware>();



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallback(async (context) =>
{
    await context.Response.WriteAsync("<a href='/products'>Get All Products</a>");
});

app.Run();

public partial class Program { }