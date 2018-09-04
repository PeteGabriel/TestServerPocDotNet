using Microsoft.EntityFrameworkCore;
using Repository.model;

namespace Repository
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
    }
}