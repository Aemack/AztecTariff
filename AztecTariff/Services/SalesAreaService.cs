using AztecTariff.Data;
using AztecTariff.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AztecTariff.Services
{
    public class SalesAreaService
    {
        private readonly ApplicationDBContext _dbContext;
        ProductService _productService;
        CategoryService _categoryService;

        public SalesAreaService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
            _productService = new ProductService(dbContext);
            _categoryService = new CategoryService(dbContext);
        }

        public async Task PopulateSalesAreaTable()
        {
            _dbContext.SalesAreas.RemoveRange(_dbContext.SalesAreas);
            await _dbContext.SaveChangesAsync();

            var sites = new List<SalesArea>();
            using (var reader = new StreamReader("Data/CSVs/Sites.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                sites = csv.GetRecords<SalesArea>().ToList();
            }

            await AddSites(sites);
        }

        public async Task AddSites(List<SalesArea> sites)
        {
            foreach(var site in sites)
            {
                _dbContext.SalesAreas.Add(site);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<FullSite>> GetAllFullSites()
        {
            var salesAreas = await _dbContext.SalesAreas.ToListAsync();
            var groupedSalesArea = salesAreas.GroupBy(g => g.SiteId);
            var fullSites = new List<FullSite>();

            foreach (var salesArea in groupedSalesArea)
            {
                var fs = new FullSite()
                {
                    SiteId = salesArea.Key,
                    SiteName = salesArea.Select(g  => g.SiteName).First(),
                    SalesAreas = await GetSitesFullSalesAreas(salesArea.Key)

                };

                fullSites.Add(fs);
            }

            return fullSites;
        }

        public async Task<List<FullSalesArea>> GetSitesFullSalesAreas(int id)
        {
            var fullsalesareas = new List<FullSalesArea>();
            var salesareas = await _dbContext.SalesAreas.Where(s => s.SiteId == id).ToListAsync();

            foreach(var sa in salesareas)
            {
                var fsa = new FullSalesArea()
                {
                    SalesAreaId = sa.SiteId,
                    SalesAreaName = sa.SAName,    
                    TariffName = sa.TariffName,
                    Included = sa.Included,
                    FooterMessage = sa.FooterMessage,
                };
                fsa.Categories = await _categoryService.GetSalesAreaCategories(sa.SalesAreaId);

                fullsalesareas.Add(fsa);
            }
            return fullsalesareas;
        }

        public async Task<List<SalesArea>> GetAllSites()
        {
            return await _dbContext.SalesAreas.ToListAsync();
        }


        public async Task<SalesArea> GetSalesArea(int salesAreaId)
        {
            return await _dbContext.SalesAreas.Where(s => s.SalesAreaId == salesAreaId).FirstAsync();
        }

        public async Task DeleteSalesArea(int salesAreaId)
        {
            _dbContext.Remove(await _dbContext.SalesAreas.Where(s => s.SalesAreaId == salesAreaId).FirstAsync());
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSalesArea(SalesArea salesArea)
        {
            var sa = await _dbContext.SalesAreas.Where(s => s.SalesAreaId == salesArea.SalesAreaId).FirstAsync();
            sa.SAName = salesArea.SAName;
            sa.TariffName = salesArea.TariffName;
            sa.SiteName = salesArea.SiteName;
            sa.SiteId = salesArea.SiteId;
            sa.Deleted = salesArea.Deleted;
            sa.Included = salesArea.Included;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSalesArea(FullSalesArea salesArea)
        {
            var sa = await _dbContext.SalesAreas.Where(s => s.SalesAreaId == salesArea.SalesAreaId).FirstAsync();
            sa.TariffName = salesArea.TariffName;
            sa.FooterMessage = salesArea.FooterMessage;
            sa.Included = salesArea.Included;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSalesAreas(List<FullSalesArea> salesAreas)
        {
            foreach(var sa in salesAreas)
            {
                await UpdateSalesArea(sa);
            }
        }

        public async Task UpdateSalesAreas(List<SalesArea> salesAreas)
        {
            foreach (var sa in salesAreas) 
            {
                await UpdateSalesArea(sa);
            }
        }

    }
}
