using AztecTariff.Data;
using AztecTariff.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AztecTariff.Services
{
    public class UpdatingService
    {
        ApplicationDBContext context;
        ProductService productService;
        PricingService pricingService;
        SalesAreaService siteService;

        List<Product> Products;
        List<SalesArea> Sites;
        List<Pricing> Pricings;

        public UpdatingService(ApplicationDBContext _context)
        {
            context = _context;
            productService = new ProductService(context);
            siteService = new SalesAreaService(context);
            pricingService = new PricingService(context);
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
            using (var reader = new StreamReader("Data\\CSVs\\Sites.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                Sites = csv.GetRecords<SalesArea>().ToList();
            }
            
            using (var reader = new StreamReader("Data\\CSVs\\DrinkList.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                Products = csv.GetRecords<Product>().ToList();
            }

            using (var reader = new StreamReader("Data\\CSVs\\Pricings.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                Pricings = csv.GetRecords<Pricing>().ToList();
            }
        }

        private async Task UpdateDatabase()
        {
            await siteService.AddSites(Sites);
            await productService.AddProducts(Products);
            await pricingService.AddPricings(Pricings);
        }

    }
}
