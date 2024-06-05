using AztecTariff.Models;
using AztecTariff.Services;
using AztecTariffModels.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
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
                settings.APIBaseAddress = s.APIBaseAddress;
                settings.PDFOuputFilesLocation = s.PDFOuputFilesLocation;
            }
            settings.IsLoggedIn = LoginDetailsValid();

            if (settings.IsLoggedIn && settings.IsValid())
            {
                categoryService = new CategoryService(dbContextFactory, settings);
                pricingService = new PricingService(dbContextFactory, settings);
                productService = new ProductService(dbContextFactory, settings);
                siteService = new SalesAreaService(dbContextFactory, settings);
                pDFDataService = new PDFDataService(dbContextFactory, settings);
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
