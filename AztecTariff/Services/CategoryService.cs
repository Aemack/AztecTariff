﻿using AztecTariff.Data;
using AztecTariff.Models;
using AztecTariffModels.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AztecTariff.Services
{
    public class CategoryService
    {
        private readonly TariffDatabaseContextFactory _dbContextFactory;
        private readonly ProductService _productService;
        private readonly PricingService _pricingService;
        

        public CategoryService(TariffDatabaseContextFactory dbContextFactory, Settings settings)
        {
            _dbContextFactory = dbContextFactory;
            _productService = new ProductService(dbContextFactory, settings);
        }

        public async Task<List<FullCategory>> GetSalesAreaCategories(int salesAreaId, DateTime selectedDate)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var categories = _dbContext.Products.Select(p => p.CategoryId).ToList().Distinct();
            var sumCats = await _dbContext.SummarizedCategories.Where(s => s.SalesAreaID == salesAreaId).ToListAsync();

            var fullcats = new List<FullCategory>();
            foreach (var category in categories)
            {
                var cat = await _dbContext.Products.Where(p => p.CategoryId == category).FirstAsync();
                var fc = new FullCategory();
                fc.Products = (await _productService.GetFullProductsByCategory(category, salesAreaId, selectedDate)).OrderBy(x => x.ProductTariffName).ToList();
                fc.TariffCategory = cat.TariffCategory;
                fc.CategoryName  = cat.CategoryName;
                fc.Id = category;
                fc.SummarizedCategory = sumCats.Where(x => x.CategoryId == category).FirstOrDefault();
                fc.IsSummarized = (fc.SummarizedCategory != null);

                fullcats.Add(fc);
            }


            return fullcats;
        }

        public async Task<List<FullCategory>> GetSalesAreaCategoriesByDate(int salesAreaId, DateTime date)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var categories = _dbContext.Products.Select(p => p.CategoryId).ToList().Distinct();
            var sumCats = await _dbContext.SummarizedCategories.Where(s => s.SalesAreaID == salesAreaId).ToListAsync();

            var fullcats = new List<FullCategory>();
            foreach (var category in categories)
            {
                var cat = await _dbContext.Products.Where(p => p.CategoryId == category).FirstAsync();
                var fc = new FullCategory();
                fc.Products = (await _productService.GetFullProductsByCategoryByDate(category, salesAreaId, date)).OrderBy(x => x.ProductTariffName).OrderBy(x => x.ProductTariffName).ToList();
                fc.TariffCategory = cat.TariffCategory;
                fc.CategoryName  = cat.CategoryName;
                fc.Id = category;
                fc.SummarizedCategory = sumCats.Where(x => x.CategoryId == category).FirstOrDefault();
                fc.IsSummarized = (fc.SummarizedCategory != null);

                fullcats.Add(fc);
            }


            return fullcats;
        }

        public async Task<List<FullCategory>> GetAllFullCategories()
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
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
            var _dbContext = _dbContextFactory.CreateDbContext();
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

        public async Task<List<SummarizedCategory>> GetSitesSummarizedCategories(int salesAreaId)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.SummarizedCategories.Where(x => x.SalesAreaID == salesAreaId).ToListAsync();
        }

        public async Task UpdateSummarizedCategory(SummarizedCategory sc)
        {

            var _dbContext = _dbContextFactory.CreateDbContext();
            var foundCat = await _dbContext.SummarizedCategories.Where(x => x.Id == sc.Id).FirstOrDefaultAsync();
            if (foundCat != null)
            {
                foundCat.MinPrice = sc.MinPrice;
                foundCat.MaxPrice = sc.MaxPrice;
                foundCat.SummaryDescription = sc.SummaryDescription;
                foundCat.Category = sc.Category;
                foundCat.CategoryId = sc.CategoryId;
                foundCat.SalesAreaID = sc.SalesAreaID;
            } else
            {
                await _dbContext.AddAsync(sc);
            }

            await _dbContext.SaveChangesAsync();
        }

        public int GetCategoryId(string category)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            return _dbContext.Products.Where(x => x.CategoryName == category).First().CategoryId;
        }

        public async Task DeleteSummarizedCategory(FullCategory category, int id)
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            var foundEntry = await _dbContext.SummarizedCategories.Where(x => x.SalesAreaID == id && x.Category == category.CategoryName).FirstOrDefaultAsync();
            if (foundEntry != null)
            {
                _dbContext.SummarizedCategories.Remove(foundEntry);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ClearSummarizedCategoriedTable()
        {
            var _dbContext = _dbContextFactory.CreateDbContext();
            _dbContext.SummarizedCategories.RemoveRange(_dbContext.SummarizedCategories);
            await _dbContext.SaveChangesAsync();
        }
    }   
}
