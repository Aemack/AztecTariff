using AztecTariff.Data;
using AztecTariff.Models;
using AztecTariffModels.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using RestSharp;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;

namespace AztecTariff.Services
{
    public class ProductService
    {
        private readonly TariffDatabaseContextFactory _dbContextFactory;
        private PricingService _pricingService;
        Settings _settings;
        public ProductService(TariffDatabaseContextFactory dbContextFactory, Settings settings)
        {
            _settings = settings;
            _dbContextFactory = dbContextFactory;
            _pricingService = new PricingService(dbContextFactory, settings);

        }


        public async Task<List<Product>> GetAllProducts()
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<List<Product>> GetAllIncludedProducts()
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.Products.Where(p => p.Included == true).ToListAsync();
        }

        public async Task<Product> GetProductById(int id)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.Products.Where(p => p.ProductId == id).FirstOrDefaultAsync();
        }

        public List<Product> GetProductsByCategory(int categoryId)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            return _dbContext.Products.Where(p => p.CategoryId == categoryId).ToList();
        }

        public List<Product> GetIncludedProductsByCategory(int categoryId)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            return _dbContext.Products.Where(p => p.CategoryId == categoryId && p.Included).ToList();
        }


        public async Task AddProduct(Product product)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product product)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var prodToUpdate = _dbContext.Products.Where(p => p.ProductId == product.ProductId).First();
            prodToUpdate.ProductTariffName = product.ProductTariffName;
            prodToUpdate.Included = product.Included;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProduct(FullProduct product)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var prodToUpdate = _dbContext.Products.Where(p => p.ProductId == product.ProductId).First();
            prodToUpdate.ProductTariffName = product.ProductTariffName;
            prodToUpdate.Included = product.Included;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProducts(List<FullProduct> products)
        {
            foreach(var p in products)
            {
                await UpdateProduct(p);
            }
        }

        public async Task DeleteProduct(int id)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            _dbContext.Products.Remove(_dbContext.Products.Where(p => p.ProductId == id).First()); ;
            await _dbContext.SaveChangesAsync();
        }



        public async Task AddProducts(List<Product> products)
        {
            foreach(var p in products)
            {
                await AddProduct(p);
            }
        }

        public async Task<List<FullProduct>> GetFullProductsByCategory(int category, int salesAreaId, DateTime selectedDate)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var fullProds = new List<FullProduct>();
            var prods = await _dbContext.Products.Where(p => p.CategoryId == category).ToListAsync();
            foreach(var prod in prods)
            {
                var fp = new FullProduct()
                {
                    ABV = prod.ABV,
                    Included = prod.Included,
                    Portion = prod.Portion,
                    Price = await _pricingService.GetProductPrice(prod.ProductId, salesAreaId, selectedDate),
                    ProdName = prod.ProdName,
                    ProductId = prod.ProductId,
                    ProductTariffName = prod.ProductTariffName,
                    IncludeInPDF = true,
                };

                fullProds.Add(fp);
            }

            return fullProds;
        }
        

        public async Task<List<FullProduct>> GetFullProductsByCategoryByDate(int category, int salesAreaId, DateTime date)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            try
            {
                var fullProds = new List<FullProduct>();
                var prods = await _dbContext.Products.Where(p => p.CategoryId == category).ToListAsync();
                foreach (var prod in prods)
                {
                    try
                    {
                        var fp = new FullProduct()
                        {
                            ABV = prod.ABV,
                            Included = prod.Included,
                            Portion = prod.Portion,
                            Price = await _pricingService.GetProductPriceByDate(prod.ProductId, salesAreaId, date),
                            ProdName = prod.ProdName,
                            ProductId = prod.ProductId,
                            ProductTariffName = prod.ProductTariffName,
                            IncludeInPDF = true,
                        };

                        fullProds.Add(fp);
                    }
                    catch
                    {
                        continue;
                    }
                }

                return fullProds;
            } catch (Exception ex)
            {

                return new List<FullProduct>();
            }
        }

        public async Task<List<FullProduct>> GetFullProducts(int category)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var prods = await _dbContext.Products.Where(p => p.CategoryId == category).ToListAsync();
            var fullprods = new List<FullProduct>();
            foreach (var prod in prods)
            {
                var fp = new FullProduct()
                {
                    ABV = prod.ABV,
                    Included = prod.Included,
                    Portion = prod.Portion,
                    ProdName = prod.ProdName,
                    ProductId = prod.ProductId,
                    ProductTariffName = prod.ProductTariffName,
                };
                fullprods.Add(fp);
            }
            return fullprods;
        }
    }
}
