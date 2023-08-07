using AztecTariff.Data;
using AztecTariff.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AztecTariff.Services
{
    public class CategoryService
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly ProductService _productService;
        private readonly PricingService _pricingService;

        public CategoryService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
            _productService = new ProductService(dbContext);
        }

        public async Task<List<FullCategory>> GetSalesAreaCategories(int salesAreaId)
        {
            var categories = _dbContext.Products.Select(p => p.CategoryId).ToList().Distinct();
            var fullcats = new List<FullCategory>();
            foreach (var category in categories)
            {
                var cat = await _dbContext.Products.Where(p => p.CategoryId == category).FirstAsync();
                var fc = new FullCategory();
                fc.Products = await _productService.GetFullProductsByCategory(category, salesAreaId);
                fc.TariffCategory = cat.TariffCategory;
                fc.CategoryName  = cat.CategoryName;
                fc.Id = category;
                fullcats.Add(fc);
            }
            return fullcats;
        }

        public async Task<List<FullCategory>> GetAllFullCategories()
        {
            var categories = _dbContext.Products.Select(p => p.CategoryId).ToList().Distinct();
            var fullcats = new List<FullCategory>();
            foreach (var category in categories)
            {
                var cat = await _dbContext.Products.Where(p => p.CategoryId == category).FirstAsync();
                var fc = new FullCategory();
                fc.Products = await _productService.GetFullProducts(category);
                fc.TariffCategory = cat.TariffCategory;
                fc.CategoryName = cat.CategoryName;
                fc.Id = category;
                fullcats.Add(fc);
            }
            return fullcats;
        }

        public async Task UpdateCategories(List<FullCategory> fullcats)
        {
            var prods = await _productService.GetAllProducts();
            foreach (var fullcat in fullcats)
            {
                var matchingProds = prods.Where(p => p.CategoryId == fullcat.Id).ToList();
                foreach(var p in matchingProds)
                {
                    p.TariffCategory = fullcat.TariffCategory;
                    await _productService.UpdateProduct(p);
                }
            }
            await _dbContext.SaveChangesAsync();
        }
    }   
}
