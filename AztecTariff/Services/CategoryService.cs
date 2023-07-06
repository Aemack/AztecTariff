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

        public CategoryService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
            _productService = new ProductService(dbContext);
        }

        public async Task PopulateCategoriesTable()
        {
            _dbContext.Categories.RemoveRange(_dbContext.Categories);
            await _dbContext.SaveChangesAsync();

            var categories = new List<Category>();
            using (var reader = new StreamReader("Data/CSVs/Categories.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                categories = csv.GetRecords<Category>().ToList();
            }
            await AddCategories(categories);
        }

        public async Task AddCategory(Category category)
        {
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();
        }
        public async Task AddCategories(List<Category> categories)
        {
            foreach(var cat in categories)
            {
                _dbContext.Categories.Add(cat);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<FullCategory>> GetAllFullCategories()
        {
            var cats = await _dbContext.Categories.ToListAsync();
            var fullCats = new List<FullCategory>();
            foreach(var cat in cats)
            {
                fullCats.Add(ToFullCategory(cat));
            }
            return fullCats;
        }

        public async Task<List<Category>> GetAllCategories()
        {
            return await _dbContext.Categories.ToListAsync();
        }

        public async Task<FullCategory> GetFullCategory(int CategoryId)
        {
            return ToFullCategory(await GetCategory(CategoryId));
        }

        public async Task<Category> GetCategory(int CategoryId)
        {
            return await _dbContext.Categories.Where(c => c.APIId == CategoryId).FirstOrDefaultAsync();
        }

        public async Task DeleteCategory(int CategoryId)
        {
            _dbContext.Categories.Remove(_dbContext.Categories.Where(c => c.APIId == CategoryId).First());
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateCategory(Category category)
        {
            var catToUpdate = _dbContext.Categories.Where(c => c.APIId == category.APIId).First();
            catToUpdate.Name = category.Name;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateCategory(FullCategory category)
        {
            var catToUpdate = _dbContext.Categories.Where(c => c.APIId == category.APIId).First();
            catToUpdate.Name = category.Name;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateCategories(List<FullCategory> cats)
        {
            foreach(var category in cats)
            {
                await UpdateCategory(category); 
            }
            Thread.Sleep(1000);
        }

        public async Task UpdateCategories(List<Category> cats)
        {
            foreach (var category in cats)
            {
                await UpdateCategory(category);
            }
        }

        public async Task<List<FullCategory>> GetFullCategoriesBySite(int siteId)
        {
            var allcats = await GetAllCategories();
            var products = await _productService.GetProductsBySite(siteId);
            var fullCats = new List<FullCategory>();
            
            foreach(var cat in allcats)
            {
                var fullcat = new FullCategory()
                {
                    Name = cat.Name,
                    APIId = cat.APIId,
                    Products = products.Where(p => p.CategoryId == cat.APIId && p.Included).ToList(),
                };

                fullCats.Add(fullcat);
            }

            return fullCats;
        }

        private FullCategory ToFullCategory(Category cat)
        {
            var fullCat = new FullCategory();
            fullCat.Name = cat.Name;
            fullCat.Products = _productService.GetProductsByCategory(cat.APIId);
            fullCat.APIId = cat.APIId;
            return fullCat;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
