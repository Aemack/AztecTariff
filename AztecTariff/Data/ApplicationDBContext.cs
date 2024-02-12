using AztecTariff.Models;
using Microsoft.EntityFrameworkCore;

namespace AztecTariff.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("DataSource = AztecTariff.db;");

        public DbSet<Product> Products { get; set; }
        public DbSet<Pricing> Pricing { get; set; }
        public DbSet<SalesArea> SalesAreas { get; set; }
        public DbSet<PDFData> PDFData { get; set; }
        public DbSet<PDFProduct> PDFProducts { get; set; }
        public DbSet<SummarizedCategory> SummarizedCategories { get; set; }

    }
}
