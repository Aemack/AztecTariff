using AztecTariff.Models;
using CsvHelper;
using System;
using System.Formats.Asn1;
using System.Globalization;
using System.Net.NetworkInformation;

namespace AztecTariff.Services
{
    public class DrinkListCreatorService
    {
        string[] words = new string[]
        {
            "Anchor", "Angel", "Arms", "Barrel", "Bear", "Bell", "Black", "Blue",
            "Boar", "Brewery", "Bridge", "Bull", "Castle", "Coach", "Cottage",
            "Crown", "Dog", "Dragon", "Duke", "Eagle", "Falcon", "Ferry", "Fisherman",
            "Flower", "Fox", "Gate", "George", "Globe", "Goat", "Golden", "Green",
            "Greyhound", "Griffin", "Hare", "Harp", "Horse", "Jolly", "King's", "Lamb",
            "Leopard", "Lion", "Malt", "Mason's", "Mermaid", "Mitre", "Nag's", "New",
            "Oak", "Old", "Otter", "Owl", "Park", "Phoenix", "Plough", "Prince", "Punch", "Queen's", "Railway", "Ram", "Red", "Robin",
            "Rose", "Royal", "Sailor's", "Stag", "Star", "Station", "Swan", "Three Tuns",
            "Tiger", "Traveller's", "Turk's Head", "Unicorn", "Victoria", "Wagon", "Waterman's", "White",
            "Windmill", "Woolpack", "Yew Tree", "Yorkshire Grey", "Pangalactic", "Gargleblaster"
        };

        string[] headings = new string[]
        {
            "Draught Beer & Cider","Soft Drinks","Mineral Water","Apertifs & Fortified","Bottled Beer & Cider","Cocktails Alcoholic","Wine"
            ,"Mixers","Spirits","Cocktails Non-Alc"
        };

        string[] pubNameWords = new string[]
                {
                    "Anchor", "Angel", "Arms", "Barrel", "Bear", "Bee", "Bell", "Bird", "Black", "Blue",
                    "Boar", "Bottle", "Brewery", "Bridge", "Buck", "Castle", "Cat", "Coach", "Cock", "Cottage",
                    "Crown", "Dog", "Dragon", "Duck", "Eagle", "Falcon", "Fish", "Fox", "Golden", "Goose",
                    "Green", "Griffin", "Hare", "Horse", "Inn", "Jug", "King's", "Lamb", "Lion", "Maiden",
                    "Malt", "Moon", "Oak", "Pig", "Prince", "Queen's", "Rabbit", "Red", "Rooster", "Rose",
                    "Royal", "Sailor", "Ship", "Star", "Stag", "Swan", "Three", "Tiger", "Unicorn", "Victoria",
                    "Wagon", "Wheat", "White", "Wild", "Willow", "Windmill", "Witch", "Wizard", "Wolf", "Wood",
                    "Yellow", "Zebra", "Abbey", "Arms", "Cask", "Cellar", "Chequers", "Cottage", "Cross", "Crown",
                    "Duke", "Fountain", "Gate", "Globe", "Grapes", "Harbour", "Hare", "Horn", "Hound", "Inn",
                    "Mill", "Phoenix", "Plough", "Pump", "Railway", "Rat", "Sailor", "Smith", "Tavern", "Wine"
                };

        string[] pubNameEndings = new string[]
            {
                "Inn", "Tavern", "Alehouse", "Arms", "Lounge", "Taproom", "Saloon", "Brewery"
            };

        string[] salesAreaNames = new string[]
        {
            "Pub", "Restaurant", "Garden", "Hotel", "Cafe","Bar"
        };


        List<Pricing> Pricings = new List<Pricing>();
        List<Product> Products = new List<Product>();
        List<SalesArea> Sites = new List<SalesArea>();

        public void CreateCSVFiles()
        {
            CreateSiteListCSV(3);
            var r = new Random();
            CreateDrinkListCSV(90);
            //CreateDrinkListCSV(r.Next(50,100));
            CreatePricingList();
        }

        private void CreatePricingList()
        {
            Pricings = new List<Pricing>();
            var r = new Random();
            int i = 0;
            foreach(var site in Sites)
            {
                foreach (var prod in Products) 
                {
                    i++;
                    var p = new Pricing()
                    {
                        Id = i,
                        SalesAreaId = site.SalesAreaId,
                        EstateId = site.EstateId,
                        Price = r.Next(10, 100) / 10m,
                        ProductId = prod.ProductId
                    };

                    Pricings.Add(p);


                }
            }

            using (var writer = new StreamWriter("Data/CSVs/Pricings.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(Pricings);
            }
        }

        private void CreateSiteListCSV(int num)
        {
            Sites = new List<SalesArea>();

            var r = new Random();
            int idcount = 1;
            for(int i = 0; i < num; i++)
            {
                var siteName = $"{GenerateName()} {pubNameEndings[r.Next(0, pubNameEndings.Length - 1)]}";
                var estateid = 34;

                for(int j = 0; j < r.Next(2,5); j++)
                {
                    SalesArea site = new SalesArea()
                    {
                        EstateId = estateid,
                        SiteName = siteName,
                        SiteId = i+1,
                        SalesAreaId = idcount,
                        SAName = salesAreaNames[j],
                        Deleted = false,
                        Included = true,
                        TariffName = $"{salesAreaNames[j]} Prices",
                    };
                    Sites.Add(site);
                    idcount++;
                }
            }

            using (var writer = new StreamWriter("Data/CSVs/Sites.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(Sites);
            }
        }

        private string GenerateName()
        {
            Random r = new Random();
            var name = "";
            for (int j = 0; j < r.Next(1, 2); j++)
            {
                name += $" {words[r.Next(0, words.Length - 1)]}";
            }

            return name.Trim();
        }

        private void CreateDrinkListCSV(int num)
        {
            var r = new Random();
            Products = new List<Product>();
            int entityCode = 1000000000;
            for (int i = 0; i < num; i++)
            {
                var newProd = new Product()
                {
                    EstateId = 34,
                    Portion = 1,
                    Included = true,
                    EntityCode = entityCode.ToString(),
                };
                entityCode++;

                newProd.ProductId = long.Parse($"{newProd.Portion}{newProd.EntityCode}");
                var drinkName = GenerateName();


                int randNum = r.Next(0, headings.Count()-1);
                newProd.CategoryId = randNum;
                var cat = headings[randNum];
                newProd.CategoryName = ShorthandText(cat);
                newProd.TariffCategory = cat;
                switch (randNum)
                {
                    case 0:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Cider" : " Beer";
                        newProd.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 1:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Cola" : " Fizz";
                        break;
                    case 2:
                        drinkName += " Water";
                        break;
                    case 3:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Amontilado" : "cello";
                        newProd.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 4:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Cider" : " Beer";
                        newProd.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 5:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " On The Rocks" : " Fishbowl";
                        newProd.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 6:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Champagne" : " Chardonay";
                        newProd.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 7:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Orange Juice" : " Tonic Water";
                        break;
                    case 8:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Vodka" : " Gin";
                        newProd.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 9:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Temple" : " Virgin";
                        break;
                }

                if (r.Next(10) > 9)
                {
                    var differentPortionProd = new Product()
                    {
                        EstateId = 34,
                        Portion = 2,
                        Included = true,
                        EntityCode = entityCode.ToString(),
                        ProdName = ShorthandText(drinkName.Trim()),
                        ABV = newProd.ABV,
                        CategoryId = newProd.CategoryId,
                        CategoryName = newProd.CategoryName,
                        ProductTariffName =  drinkName.Trim(),
                        TariffCategory = newProd.TariffCategory,
                    };
                    differentPortionProd.ProductId = int.Parse($"{differentPortionProd.Portion}{differentPortionProd.EntityCode}");
                    entityCode++;

                    Products.Add(differentPortionProd);

                }

                newProd.ProdName = ShorthandText(drinkName.Trim());
                newProd.ProductTariffName = drinkName.Trim();
                newProd.Included = true;
                Products.Add(newProd);
            }
            using (var writer = new StreamWriter("Data/CSVs/DrinkList.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(Products);
            }
        }

        private string ShorthandText(string v)
        {
            return v.Replace("a", "").Replace("e", "").Replace("i", "").Replace("o", "").Replace("u", "").Replace(" ", "");
        }
    }
}
