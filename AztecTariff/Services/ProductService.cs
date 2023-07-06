using AztecTariff.Data;
using AztecTariff.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AztecTariff.Services
{
    public class ProductService
    {
        private readonly ApplicationDBContext _dbContext;

        public ProductService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task PopulateProductsTable()
        {
            _dbContext.Products.RemoveRange(_dbContext.Products);
            await _dbContext.SaveChangesAsync();

            var products = new List<Product>();
            using (var reader = new StreamReader("Data/CSVs/DrinkList.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                products = csv.GetRecords<Product>().ToList();
            }

            foreach (var product in products)
            {
                _dbContext.Products.Add(product);    
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task PopulateProductsMappingTable()
        {
            _dbContext.SiteProductMapping.RemoveRange(_dbContext.SiteProductMapping);
            await _dbContext.SaveChangesAsync();

            var productMappings = new List<SiteProductMapping>();
            var products = await GetAllProducts();

            foreach (var site in _dbContext.Sites)
            {
                foreach(var prod in products)
                {
                    var newSiteMap = new SiteProductMapping()
                    {
                        SiteId = site.APIId,
                        ProductId = prod.APIId,

                    };
                    _dbContext.SiteProductMapping.Add(newSiteMap);
                }
                
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Product>> GetAllProducts()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<List<Product>> GetAllIncludedProducts()
        {
            return await _dbContext.Products.Where(p => p.Included == true).ToListAsync();
        }

        public async Task<Product> GetProductById(int id)
        {
            return await _dbContext.Products.Where(p => p.APIId == id).FirstOrDefaultAsync();
        }

        public List<Product> GetProductsByCategory(int categoryId) 
        {
            return _dbContext.Products.Where(p => p.CategoryId == categoryId).ToList();
        }

        public List<Product> GetIncludedProductsByCategory(int categoryId) 
        {
            return _dbContext.Products.Where(p => p.CategoryId == categoryId && p.Included).ToList();
        }

        public async Task<List<Product>> GetProductsBySite(int siteId)
        {
            var products = new List<Product>();
            var mapping = await _dbContext.SiteProductMapping.Where(sm => sm.SiteId == siteId).ToListAsync();
            foreach(var m in mapping)
            {
                products.Add(_dbContext.Products.Where(p => p.APIId == m.ProductId).FirstOrDefault());
            }

            return products;
        }

        public async Task AddProduct(Product product)
        {
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product product)
        {
            var prodToUpdate = _dbContext.Products.Where(p => p.APIId == product.APIId).FirstOrDefault();
            prodToUpdate.ProductName = product.ProductName;
            prodToUpdate.Included = product.Included;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteProduct(int id)
        {
            _dbContext.Products.Remove(_dbContext.Products.Where(p => p.APIId == id).First());
            await _dbContext.SaveChangesAsync();
        }
    }
}
