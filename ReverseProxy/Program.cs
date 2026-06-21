using Microsoft.EntityFrameworkCore;
using ReverseProxy;
using ReverseProxy.Authentication;
using ReverseProxy.Database;
using ReverseProxy.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IRedisCaching, RedisService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "cache";
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<ReverseProxyMiddleware>();

app.UseMiddleware<ApiKeyAuthentication>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallback(async (context) =>
{
    await context.Response.WriteAsync("<a href='/products'>Get All Products</a>");
});

app.Run();