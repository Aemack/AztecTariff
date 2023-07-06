using AztecTariff.Models;
using Microsoft.EntityFrameworkCore;

namespace AztecTariff.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("DataSource = AztecTariff.db;");

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<SiteProductMapping> SiteProductMapping { get; set; }

    }
}
