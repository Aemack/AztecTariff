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

        public async Task<double> GetProductPrice(long productId, int salesareaId, DateTime selectedDate)
        {
            Pricing x = new Pricing();
            if (selectedDate == DateTime.MinValue)
            {
                x = await _dbContext.Pricings.Where(p => p.ProductId == productId && p.SalesAreaId == salesareaId).FirstOrDefaultAsync();
            }
            else
            {
                x = await _dbContext.Pricings.Where(p => p.ProductId == productId && p.SalesAreaId == salesareaId && p.StartDate <= selectedDate && p.EndDate >= selectedDate).FirstOrDefaultAsync();
            }

            return x.Price;
        }

        public async Task<Pricing> GetProductPricing(long productId, int salesareaId, DateTime selectedDate)
        {
            return await _dbContext.Pricings.Where(p => p.ProductId == productId && p.SalesAreaId == salesareaId && p.StartDate <= selectedDate && p.EndDate >= selectedDate).FirstOrDefaultAsync();
            
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

        public async Task UpdatePricingByDate(long ProductId, double Price, DateTime CurrentDate)
        {
            var p = await _dbContext.Pricings.Where(pr => pr.ProductId == ProductId && pr.StartDate <= CurrentDate && pr.EndDate >= CurrentDate).FirstOrDefaultAsync();
            if(p == null ) return;
            p.Price = Price;

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
            var x = await _dbContext.Pricings.Where(p => p.ProductId == productId && p.SalesAreaId == salesAreaId && (p.StartDate <= date && p.EndDate >= date)).FirstOrDefaultAsync();
            return x.Price;
        }

        public DateTime GetMostRecentDate()
        {
            var x = _dbContext.Pricings.ToList().MaxBy(d => d.EndDate);
            return x.EndDate;
        }

        public DateTime GetEarliestDate()
        {
            var x = _dbContext.Pricings.ToList().MinBy(d => d.StartDate);
            return x.EndDate;
        }
    }
}
