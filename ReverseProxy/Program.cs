using ReverseProxy;
using ReverseProxy.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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