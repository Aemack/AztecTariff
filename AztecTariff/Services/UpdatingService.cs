using AztecTariff.Data;
using AztecTariff.Models;
using AztecTariffModels.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Globalization;

namespace AztecTariff.Services
{
    public class UpdatingService
    {
        TariffDatabaseContextFactory contextFactory;
        ProductService productService;
        PricingService pricingService;
        SalesAreaService siteService;

        List<Product> Products;
        List<SalesArea> Sites;
        List<Pricing> Pricings;
        Settings _settings;
        public UpdatingService(TariffDatabaseContextFactory _dbContextFactory, Settings settings)
        {
            _settings = settings;
            contextFactory = _dbContextFactory;
            pricingService = new PricingService(contextFactory, settings);
            productService = new ProductService(contextFactory, settings);
            siteService = new SalesAreaService(contextFactory, settings);
            
        }

        public async void Update()
        {
            GetData();
            await UpdateDatabase();
        }

        private void GetData()
        {
            GetDataFromCSVs();
        }

        private void GetDataFromCSVs()
        {
            using (var reader = new StreamReader(Path.Combine(_settings.CSVFileLocation, "CSVs/Sites.csv")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                Sites = csv.GetRecords<SalesArea>().ToList();
            }

            using (var reader = new StreamReader(Path.Combine(_settings.CSVFileLocation, "CSVs/DrinkList.csv")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                Products = csv.GetRecords<Product>().ToList();
            }

            using (var reader = new StreamReader(Path.Combine(_settings.CSVFileLocation, "CSVs/Pricings.csv")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                Pricings = csv.GetRecords<Pricing>().ToList();
            }
        }

        private async Task UpdateDatabase()
        {
            await siteService.AddSalesAreas(Sites);
            await productService.AddProducts(Products);
            await pricingService.AddPricings(Pricings);
        }

    }
}
