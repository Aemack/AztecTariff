﻿using AztecTariff.Data;
using AztecTariff.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Globalization;
using System.Text.Json;

namespace AztecTariff.Services
{
    public class PricingService
    {
        private readonly ApplicationDBContext _dbContext;
        Settings _settings;
        public PricingService(ApplicationDBContext dbContext, Settings settings)
        {
            _settings = settings;
            _dbContext = dbContext;
        }
         
        public async Task<List<Pricing>> GetAllPricing()
        {
            return await _dbContext.Pricing.ToListAsync();
        }

        public async Task<decimal> GetProductPrice(long productId, int salesareaId)
        {
            var x =  await _dbContext.Pricing.Where(p => p.ProductId == productId && p.SalesAreaId == salesareaId).FirstOrDefaultAsync();
            return x.Price;
        }

        public async Task<Pricing> GetProductPricing(long productId, int salesareaId)
        {
            return await _dbContext.Pricing.Where(p => p.ProductId == productId && p.SalesAreaId == salesareaId).FirstOrDefaultAsync();
            
        }

        public async Task AddPricing(Pricing pricing)
        {
            await _dbContext.Pricing.AddAsync(pricing);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePricing(Pricing pricing)
        {
            var p = await _dbContext.Pricing.Where(pr => pr.Id == pricing.Id).FirstOrDefaultAsync();
            p.Price = pricing.Price;
            p.ProductId = pricing.ProductId;
            p.SalesAreaId = pricing.SalesAreaId;
            p.EstateId = pricing.EstateId;

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemovePricing(Pricing pricing)
        {
            _dbContext.Pricing.Remove(pricing);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddPricings(List<Pricing> pricings)
        {
            foreach(var p  in pricings) 
            { 
               await AddPricing(p);
            }
        }

        public async Task PopulatePricingsTable()
        {
            _dbContext.Pricing.RemoveRange(_dbContext.Pricing);
            await _dbContext.SaveChangesAsync();

            var options = new RestClientOptions(_settings.APIBaseAddress)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest($"api/Pricings", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);

            JsonSerializerOptions jsoptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var pricings = JsonSerializer.Deserialize<List<Pricing>>(response.Content, jsoptions);

            foreach (var pricing in pricings)
            {
                _dbContext.Pricing.Add(pricing);
            }

            await _dbContext.SaveChangesAsync();
        }

        public List<Pricing> GetPricingBySA(int? salesAreaId)
        {
            return _dbContext.Pricing.Where(p => p.SalesAreaId == salesAreaId).ToList();
        }

        public async Task DeletePricingBySA(int salesAreaId)
        {
            _dbContext.Pricing.RemoveRange(_dbContext.Pricing.Where(p => p.SalesAreaId == salesAreaId));
            await _dbContext.SaveChangesAsync();
        }
    }
}
