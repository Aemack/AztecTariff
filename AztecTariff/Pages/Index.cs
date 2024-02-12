using AztecTariff.Models;
using AztecTariff.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Xml.Serialization;

namespace AztecTariff.Pages
{
    public partial class Index : BasePageClass
    {
        #region Props
        CategoryService categoryService;
        SalesAreaService siteService;
        ProductService productService;
        PricingService pricingService;

        bool isLoading;
        string username;
        string password;
        bool usernameError;
        bool passwordError;

        string usernameErrorClass => (usernameError) ? "border-danger border" : "";
        string passwordErrorClass => (passwordError) ? "border-danger border" : "";
        #endregion
        protected override void OnInitialized()
        {
            //settings.TemplateFolderLocation = "C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\New folder";
            //settings.CSVFileLocation = "C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\New folder";
            //settings.WordFileLocation = "C:\Users\AdamM2\OneDrive - Zonal Retail Data Systems Limited\Desktop\TariffNotesNStuff\New folder\Test.docx";
            //settings.LibreLocation = @"C:\Users\AdamM2\Downloads\LibreOfficePortable\LibreOfficePortable.exe";




            usernameError = false;
            passwordError = false;

        }

        private async Task LogIn()
        {
            isLoading = true;
            await Task.Delay(1);
            if (!File.Exists("settings.xml"))
            {
                File.Create("settings.xml").Close();
            }
            var s = LoadSettingsFromXml("settings.xml");
            if (s != null)
            {
                settings.CSVFileLocation = s.CSVFileLocation;
                settings.WordFileLocation = s.WordFileLocation;
                settings.LibreLocation = s.LibreLocation;
                settings.TemplateFolderLocation = s.TemplateFolderLocation;
                settings.APIBaseAddress = s.APIBaseAddress;
            }
            settings.IsLoggedIn = LoginDetailsValid();

            if (settings.IsLoggedIn && settings.IsValid())
            {
                categoryService = new CategoryService(DbFactory.CreateDbContext(), settings);
                pricingService = new PricingService(DbFactory.CreateDbContext(), settings);
                productService = new ProductService(DbFactory.CreateDbContext(), settings);
                siteService = new SalesAreaService(DbFactory.CreateDbContext(), settings);
                pDFDataService = new PDFDataService(DbFactory.CreateDbContext(), settings);
                await RepopulateDatabase();
                nav.NavigateTo("/SiteTariff");
            }
            else if (!settings.IsValid())
            {
                nav.NavigateTo("/Settings");
            }

            isLoading = false;
            await Task.Delay(1);
        }

        public static Settings LoadSettingsFromXml(string filePath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                using (StreamReader reader = new StreamReader(filePath))
                {
                    return (Settings)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return null;
            }
        }

        private async Task RepopulateDatabase()
        {
            var ds = new DrinkListCreatorService(settings);
            var allProds = await productService.GetAllProducts();
            //if (allProds.Count == 0)
            //{
            //ds.CreateCSVFiles();
            await siteService.PopulateSalesAreaTable();
            await productService.PopulateProductsTable();
            await pricingService.PopulatePricingsTable();
            await pDFDataService.ClearPDFDataTable();
            await pDFDataService.ClearPDFProductTable();
            await categoryService.ClearSummarizedCategoriedTable();
            //}
        }

        public bool LoginDetailsValid()
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                usernameError = true;
            }
            else
            {
                usernameError = false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                passwordError = true;
            }
            else
            {
                passwordError = false;
            }


            if (usernameError || passwordError)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async void Enter(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                await LogIn();
            }
        }

    }
}
