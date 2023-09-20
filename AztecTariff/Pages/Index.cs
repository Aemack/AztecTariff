using AztecTariff.Services;
using Microsoft.AspNetCore.Components.Web;

namespace AztecTariff.Pages
{
    public partial class Index : BasePageClass
    {
        #region Props
        CategoryService categoryService;
        SalesAreaService siteService;
        ProductService productService;
        PricingService pricingService;
        

        bool isLoading; string username;
        string password;
        bool usernameError;
        bool passwordError;

        string usernameErrorClass => (usernameError) ? "border-danger border" : "";
        string passwordErrorClass => (passwordError) ? "border-danger border" : "";
        #endregion
        protected override void OnInitialized()
        {
            categoryService = new CategoryService(DbFactory.CreateDbContext());
            pricingService = new PricingService(DbFactory.CreateDbContext());
            productService = new ProductService(DbFactory.CreateDbContext());
            siteService = new SalesAreaService(DbFactory.CreateDbContext());
            usernameError = false;
            passwordError = false;
            //PopulateDatabase();
        }

        private async Task LogIn()
        {
            isLoading = true;
            await Task.Delay(1);
            Thread.Sleep(5000);
            settings.IsLoggedIn = LoginDetailsValid();
            isLoading = false;
            await Task.Delay(1);
            if (settings.IsLoggedIn)
            {
                nav.NavigateTo("/SiteTariff");
            }

        }

        private async void PopulateDatabase()
        {
            var ds = new DrinkListCreatorService();
            ds.CreateCSVFiles();
            await productService.PopulateProductsTable();
            await pricingService.PopulatePricingsTable();
            await siteService.PopulateSalesAreaTable();
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
