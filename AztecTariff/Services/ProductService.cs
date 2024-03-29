﻿using AztecTariff.Data;
using AztecTariff.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Globalization;
using System.Text.Json;

namespace AztecTariff.Services
{
    public class ProductService
    {
        private readonly ApplicationDBContext _dbContext;
        private PricingService _pricingService;
        Settings _settings;
        public ProductService(ApplicationDBContext dbContext, Settings settings)
        {
            _settings = settings;
            _dbContext = dbContext;
            _pricingService = new PricingService(dbContext, settings);

        }


        public async Task PopulateProductsTable()
        {
            _dbContext.Products.RemoveRange(_dbContext.Products);
            await _dbContext.SaveChangesAsync();

            var options = new RestClientOptions(_settings.APIBaseAddress)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest($"api/Products", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);

            JsonSerializerOptions jsoptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<Product> products = JsonSerializer.Deserialize<List<Product>>(response.Content, jsoptions);

            foreach (var product in products)
            {
                _dbContext.Products.Add(product);    
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
            return await _dbContext.Products.Where(p => p.ProductId == id).FirstOrDefaultAsync();
        }

        public List<Product> GetProductsByCategory(int categoryId) 
        {
            return _dbContext.Products.Where(p => p.CategoryId == categoryId).ToList();
        }

        public List<Product> GetIncludedProductsByCategory(int categoryId) 
        {
            return _dbContext.Products.Where(p => p.CategoryId == categoryId && p.Included).ToList();
        }


        public async Task AddProduct(Product product)
        {
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product product)
        {
            var prodToUpdate = _dbContext.Products.Where(p => p.ProductId == product.ProductId).First();
            prodToUpdate.ProductTariffName = product.ProductTariffName;
            prodToUpdate.Included = product.Included;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProduct(FullProduct product)
        {
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

        public async Task<List<FullProduct>> GetFullProductsByCategory(int category, int salesAreaId)
        {
            var fullProds = new List<FullProduct>();
            var prods = await _dbContext.Products.Where(p => p.CategoryId == category).ToListAsync();
            foreach(var prod in prods)
            {
                var fp = new FullProduct()
                {
                    ABV = prod.ABV,
                    Included = prod.Included,
                    Portion = prod.Portion,
                    Price = await _pricingService.GetProductPrice(prod.ProductId, salesAreaId),
                    ProdName = prod.ProdName,
                    ProductId = prod.ProductId,
                    ProductTariffName = prod.ProductTariffName,
                    IncludeInPDF = true,
                };

                fullProds.Add(fp);
            }

            return fullProds;
        }

        public async Task<List<FullProduct>> GetFullProducts(int category)
        {
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
