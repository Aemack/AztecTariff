using AztecTariff.Models;
using AztecTariffModels.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Globalization;
using System.Text.Json;

namespace AztecTariff.Services
{
    public class PricingService
    {
        private readonly TariffDatabaseContext _dbContext;
        Settings _settings;
        public PricingService(TariffDatabaseContext dbContext, Settings settings)
        {
            _settings = settings;
            _dbContext = dbContext;
        }
         
        public async Task<List<Pricing>> GetAllPricing()
        {
            return await _dbContext.Pricings.ToListAsync();
        }

        public async Task<double> GetProductPrice(long productId, int salesareaId)
        {
            var x =  await _dbContext.Pricings.Where(p => p.ProductId == productId && p.SalesAreaId == salesareaId).FirstOrDefaultAsync();
            return x.Price;
        }

        public async Task<Pricing> GetProductPricing(long productId, int salesareaId)
        {
            return await _dbContext.Pricings.Where(p => p.ProductId == productId && p.SalesAreaId == salesareaId).FirstOrDefaultAsync();
            
        }

        public async Task AddPricing(Pricing pricing)
        {
            await _dbContext.Pricings.AddAsync(pricing);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePricing(Pricing pricing)
        {
            var p = await _dbContext.Pricings.Where(pr => pr.Id == pricing.Id).FirstOrDefaultAsync();
            p.Price = pricing.Price;
            p.ProductId = pricing.ProductId;
            p.SalesAreaId = pricing.SalesAreaId;
            p.EstateId = pricing.EstateId;

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemovePricing(Pricing pricing)
        {
            _dbContext.Pricings.Remove(pricing);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddPricings(List<Pricing> pricings)
        {
            foreach(var p  in pricings) 
            { 
               await AddPricing(p);
            }
        }

        public List<Pricing> GetPricingBySA(int? salesAreaId)
        {
            return _dbContext.Pricings.Where(p => p.SalesAreaId == salesAreaId).ToList();
        }

        public async Task DeletePricingBySA(int salesAreaId)
        {
            _dbContext.Pricings.RemoveRange(_dbContext.Pricings.Where(p => p.SalesAreaId == salesAreaId));
            await _dbContext.SaveChangesAsync();
        }

        public async Task<double> GetProductPriceByDate(long productId, int salesAreaId, DateTime date)
        {
            var x = await _dbContext.Pricings.Where(p => p.ProductId == productId && p.SalesAreaId == salesAreaId && (p.StartDate <= date && p.EndDate > date)).FirstOrDefaultAsync();
            return x.Price;
        }
    }
}
