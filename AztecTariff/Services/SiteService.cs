using AztecTariff.Data;
using AztecTariff.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AztecTariff.Services
{
    public class SiteService
    {
        private readonly ApplicationDBContext _dbContext;
        ProductService _productService;
        CategoryService _categoryService;

        public SiteService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
            _productService = new ProductService(dbContext);
            _categoryService = new CategoryService(dbContext);
        }

        public async Task PopulateSiteTable()
        {
            _dbContext.Sites.RemoveRange(_dbContext.Sites);
            await _dbContext.SaveChangesAsync();

            var sites = new List<Site>();
            using (var reader = new StreamReader("Data/CSVs/Sites.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                sites = csv.GetRecords<Site>().ToList();
            }

            await AddSites(sites);
        }

        public async Task AddSites(List<Site> sites)
        {
            foreach(var site in sites)
            {
                _dbContext.Sites.Add(site);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<FullSite>> GetAllFullSites()
        {
            var sites = await GetAllSites();
            var fullSites = new List<FullSite>();   
            foreach(var site in sites)
            {
                var fullSite = new FullSite()
                {
                    Name = site.Name,
                    Id = site.APIId,
                    Categories = await _categoryService.GetFullCategoriesBySite(site.APIId)
                };
                fullSites.Add(FullSite.GetClonedInstance(fullSite));
            }

            return fullSites;

        }

        public async Task<List<Site>> GetAllSites()
        {
            return await _dbContext.Sites.ToListAsync();
        }

        public async Task<FullSite> GetFullSite(int siteId)
        {
            var site = _dbContext.Sites.Where(s => s.APIId == siteId).FirstOrDefault();
            return await ToFullSite(site);
        }

        private async Task<FullSite> ToFullSite(Site? site)
        {
            return new FullSite()
            {
                Categories = await _categoryService.GetFullCategoriesBySite(site.APIId),
                Name = site.Name
            };
        }

        public Site GetSite(int siteId)
        {
            return new Site();
        }

        public async Task DeleteSite(int siteId)
        {
            _dbContext.Sites.Remove(_dbContext.Sites.Where(s => s.APIId == siteId).FirstOrDefault());
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSite(Site site)
        {
            var siteToUpdate = _dbContext.Sites.Where(s => s.APIId == site.Id).FirstOrDefault();
            siteToUpdate.Name = site.Name;
            await _dbContext.SaveChangesAsync();

        }

        public async Task UpdateSite(FullSite site)
        {
            var siteToUpdate = _dbContext.Sites.Where(s => s.APIId == site.Id).FirstOrDefault();
            siteToUpdate.Name = site.Name;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSites(List<FullSite> sites)
        {
            foreach(var site in sites)
            {
                await UpdateSite(site);
            }
        }

        public async Task UpdateSites(List<Site> sites)
        {
            foreach (var site in sites)
            {
                await UpdateSite(site);
            }
        }

    }
}
