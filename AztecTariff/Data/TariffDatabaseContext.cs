using AztecTariffModels.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AztecTariff.Data
{
    public class TariffDatabaseContext : DbContext
    {
        public TariffDatabaseContext(DbContextOptions<TariffDatabaseContext> options) : base(options)
        {
        }

        public DbSet<LoginDetails> LoginDetails { get; set; }
        public DbSet<Pricing> Pricings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SalesArea> SalesAreas { get; set; }
        public DbSet<PDFData> PDFData { get; set; }
        public DbSet<PDFProduct> PDFProducts { get; set; }
        public DbSet<SummarizedCategory> SummarizedCategories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pricing>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<SalesArea>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<LoginDetails>()
                .HasKey(l => l.Id);
        }
    }
}

