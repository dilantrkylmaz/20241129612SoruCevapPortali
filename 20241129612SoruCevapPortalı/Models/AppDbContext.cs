using Microsoft.EntityFrameworkCore;

namespace _20241129612SoruCevapPortalı.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
    }
}
