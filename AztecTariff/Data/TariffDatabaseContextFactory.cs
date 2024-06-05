using AztecTariffModels.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace AztecTariff.Data
{
    public class TariffDatabaseContextFactory : IDbContextFactory<TariffDatabaseContext>
    {
            private readonly DbContextOptions<TariffDatabaseContext> _options;

            public TariffDatabaseContextFactory(DbContextOptions<TariffDatabaseContext> options)
            {
                _options = options;
            }

            public TariffDatabaseContext CreateDbContext()
            {
                return new TariffDatabaseContext(_options);
            }
        }
}
