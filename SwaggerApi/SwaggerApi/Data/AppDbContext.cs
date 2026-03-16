using Microsoft.EntityFrameworkCore;
using SwaggerApi.Models;

namespace SwaggerApi.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
            
        }

        // Define DbSets for your entities
        public DbSet<Product> Products { get; set; }
    }
}
