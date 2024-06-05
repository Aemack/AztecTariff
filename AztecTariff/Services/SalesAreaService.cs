using AztecTariff.Data;
using AztecTariff.Models;
using AztecTariffModels.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using RestSharp;
using System.Globalization;
using System.Text.Json;

namespace AztecTariff.Services
{
    public class SalesAreaService
    {
        private readonly TariffDatabaseContextFactory _dbContextFactory;
        ProductService _productService;
        CategoryService _categoryService;
        Settings _settings;

        public SalesAreaService(TariffDatabaseContextFactory dbContextFactory, Settings settings)
        {
            _settings = settings;
            _dbContextFactory = dbContextFactory;
            _productService = new ProductService(dbContextFactory, settings);
            _categoryService = new CategoryService(dbContextFactory, settings);
        }


        public async Task AddSalesAreas(List<SalesArea> sites)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            foreach (var site in sites)
            {
                _dbContext.SalesAreas.Add(site);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<SalesArea>> GetAllEvents()
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.SalesAreas.Where(s => s.isEvent).ToListAsync();
        }

        public async Task<List<FullSite>> GetAllFullSitesByDate(DateTime date)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var salesAreas = await _dbContext.SalesAreas.ToListAsync();

            var groupedSalesArea = salesAreas.GroupBy(g => g.SiteId);
            
            var fullSites = new List<FullSite>();

            foreach (var salesArea in groupedSalesArea)
            {
                var fs = new FullSite()
                {
                    SiteId = salesArea.Key,
                    SiteName = salesArea.Select(g => g.SiteName).First(),
                    SalesAreas = await GetSitesFullSalesAreasByDate(salesArea.Key, date)

                };

                fullSites.Add(fs);
            }

            return fullSites.OrderBy(x => x.SiteName).ToList();
        }

        public async Task<List<FullSalesArea>> GetSitesFullSalesAreasByDate(int id, DateTime date)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var fullsalesareas = new List<FullSalesArea>();
            var salesareas = await _dbContext.SalesAreas.Where(s => s.SiteId == id && !s.isEvent).ToListAsync();

            foreach (var sa in salesareas)
            {
                var fsa = new FullSalesArea()
                {
                    SalesAreaId = sa.SalesAreaId,
                    SalesAreaName = sa.SAName,
                    SiteName = sa.SiteName,
                    TariffName = sa.TariffName,
                    Included = sa.Included,
                    FooterMessage = sa.FooterMessage,
                    HeaderMessage = sa.HeaderMessage
                };


                fsa.Events = await GetEventBySalesArea(sa.SalesAreaId);
                fsa.Categories = await _categoryService.GetSalesAreaCategoriesByDate(sa.SalesAreaId, date);
                fsa.Categories = fsa.Categories.OrderBy(x => x.CategoryName).ToList();
                //fsa.Categories = await _categoryService.GetSalesAreaCategories(sa.SalesAreaId, date);

                fullsalesareas.Add(fsa);
            }
            return fullsalesareas.OrderBy(x => x.SalesAreaName).ToList(); ;
        }

        public async Task<List<FullEvent>> GetEventBySalesArea(int salesAreaId)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var ev = await _dbContext.SalesAreas.Where(s => s.OriginalSalesAreaId == salesAreaId).ToListAsync();
            return await ToFullEvents(ev);
        }

        public  async Task<List<FullEvent>> ToFullEvents(List<SalesArea> ev)
        {
            var eventsList = new List<FullEvent>();
            foreach(var s in ev)
            {
                eventsList.Add(await ToFullEvent(s));
            }

            return eventsList;
        }

        public async Task<FullEvent> ToFullEvent(SalesArea sa)
        {
            var x = new FullEvent()
            {
                OriginalSalesAreaId = (int)sa.OriginalSalesAreaId,
                SalesAreaId = sa.SalesAreaId,
                SalesAreaName = sa.SAName,
                SiteName = sa.SiteName,
                FooterMessage = sa.FooterMessage,
                HeaderMessage = sa.HeaderMessage,
                Included = sa.Included,
                TariffName = sa.TariffName
            };
            x.Categories = await _categoryService.GetSalesAreaCategories(x.SalesAreaId, DateTime.MinValue);

            return x;

        }

        public async Task<List<SalesArea>> GetAllSites()
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.SalesAreas.ToListAsync();
        }


        public async Task<SalesArea> GetSalesArea(int salesAreaId)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.SalesAreas.Where(s => s.SalesAreaId == salesAreaId).FirstAsync();
        }

        public async Task DeleteSalesArea(int salesAreaId)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            _dbContext.Remove(await _dbContext.SalesAreas.Where(s => s.SalesAreaId == salesAreaId).FirstAsync());
            await _dbContext.SaveChangesAsync();
        }


        public async Task UpdateSalesArea(SalesArea salesArea)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var sa = await _dbContext.SalesAreas.Where(s => s.SalesAreaId == salesArea.SalesAreaId).FirstAsync();
            sa.SAName = salesArea.SAName;
            sa.TariffName = salesArea.TariffName;
            sa.SiteName = salesArea.SiteName;
            sa.SiteName = salesArea.SiteName;
            sa.HeaderMessage = salesArea.HeaderMessage;
            sa.SiteId = salesArea.SiteId;
            sa.Deleted = salesArea.Deleted;
            sa.Included = salesArea.Included;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSalesArea(FullSalesArea salesArea)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var sa = await _dbContext.SalesAreas.Where(s => s.SalesAreaId == salesArea.SalesAreaId).FirstAsync();
            sa.TariffName = salesArea.TariffName;
            sa.FooterMessage = salesArea.FooterMessage;
            sa.HeaderMessage = salesArea.HeaderMessage;
            sa.Included = salesArea.Included;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSalesAreas(List<FullSalesArea> salesAreas)
        {
            foreach (var sa in salesAreas)
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

        public async Task<int> AddSalesArea(SalesArea sa)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            _dbContext.SalesAreas.Add(sa);
            await _dbContext.SaveChangesAsync();

            return sa.SalesAreaId;
        }

    }
}
