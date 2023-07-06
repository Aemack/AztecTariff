using AztecTariff.Models;
using CsvHelper;
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
                "Inn", "Tavern", "Bar", "Pub", "Alehouse", "Arms", "Lounge", "Taproom", "Saloon", "Brewery"
            };


        List<Category> Categories = new List<Category>();
        List<Product> Products = new List<Product>();
        List<Site> Sites = new List<Site>();

        public void CreateCSVFiles()
        {
            CreateSiteListCSV(8);
            CreateCategoryCSV();
            CreateDrinkListCSV(70);
        }

        private void CreateSiteListCSV(int num)
        {
            Sites = new List<Site>();
            var r = new Random();
            for(int i = 0; i < num; i++)
            {
                Site site = new Site()
                {
                    APIId = i,
                    Name = $"{GenerateName()} {pubNameEndings[r.Next(0, pubNameEndings.Length - 1)]}"
                };
                Sites.Add(site);
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

        private void CreateCategoryCSV()
        {
            Categories = new List<Category>();
            int i = 0;
            foreach (string heading in headings) 
            {
                Categories.Add(new Category()
                {
                    Name = heading,
                    APIId = i
                });
                i++;
            }

            using (var writer = new StreamWriter("Data/CSVs/Categories.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(Categories);
            }
        }


        private void CreateDrinkListCSV(int num)
        {
            var r = new Random();
            Products = new List<Product>();
            for (int i = 0; i < num; i++)
            {
                var newRec = new Product()
                {
                    APIId = i,
                    Included = true,
                    Price = (decimal)r.Next(0, 100) / (decimal)10.00,
                };

                var drinkName = GenerateName();


                int randNum = r.Next(0, headings.Count());
                newRec.CategoryId = Categories[randNum].APIId;
                switch (randNum)
                {
                    case 0:
                        drinkName += " Beer";
                        newRec.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 1:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Cola" : " Fizz";
                        break;
                    case 2:
                        drinkName += " Water";
                        break;
                    case 3:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Amontilado" : "cello";
                        newRec.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 4:
                        drinkName += " Beer";
                        newRec.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 5:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " On The Rocks" : " Fishbowl";
                        newRec.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 6:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Champagne" : " Chardonay";
                        newRec.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 7:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Orange Juice" : " Tonic Water";
                        break;
                    case 8:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Vodka" : " Gin";
                        newRec.ABV = Math.Round(r.NextDouble() * 10, 2);
                        break;
                    case 9:
                        drinkName += (r.Next(0, 10) % 2 == 0) ? " Temple" : " Virgin";
                        break;
                }

                newRec.ProductName = drinkName.Trim();
                newRec.Included = true;
                Products.Add(newRec);
            }
            using (var writer = new StreamWriter("Data/CSVs/DrinkList.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(Products);
            }
        }

    }
}
