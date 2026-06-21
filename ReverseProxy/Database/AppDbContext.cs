using Microsoft.EntityFrameworkCore;
using ReverseProxy.Models;

namespace ReverseProxy.Database
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<ApiKey> ApiKeys { get; set; }

    }
}
