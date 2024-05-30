using AztecTariff.Services;

namespace AztecTariff
{
    public static class Startup
    {
        public static void SetUpFolders()
        {
            var folderPath = @"C:\ZBS\Tariff";
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("Folder created successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating folder: {ex.Message}");
                }
            }
            var dataPath = Path.Combine(folderPath, "TariffData");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(Path.Combine(folderPath, "TariffData"));
            if (!Directory.Exists(Path.Combine(dataPath, "SalesAreas"))) Directory.CreateDirectory(Path.Combine(dataPath, "SalesAreas"));
            if (!Directory.Exists(Path.Combine(dataPath, "Products"))) Directory.CreateDirectory(Path.Combine(dataPath, "Products"));
            if (!Directory.Exists(Path.Combine(dataPath, "Pricings"))) Directory.CreateDirectory(Path.Combine(dataPath, "Pricings"));
        }


        internal static void GenerateTestData()
        {
            //DrinkListCreatorService dlcs = new DrinkListCreatorService();
            //dlcs.CreateCSVFiles();
        }
    }
}

