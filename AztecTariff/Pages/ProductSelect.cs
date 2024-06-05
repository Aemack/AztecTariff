using AztecTariff.Models;
using AztecTariff.Services;
using AztecTariff.Shared;
using AztecTariffModels.Models;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;

namespace AztecTariff.Pages
{
    public partial class ProductSelect : BasePageClass
    {
        #region Props
        Toast Toast;
        
        TelerikGrid<FullCategory> CategoryGridRef;
        TelerikGrid<FullProduct> ProductGridRef;

        List<FullCategory> Categories = new List<FullCategory>();
        List<FullCategory> OpenHeadings = new List<FullCategory>();
        List<FullProduct> SelectedProducts = new List<FullProduct>();
        
        FullCategory SelectedCategory = new FullCategory();


        bool toastDisplayed;
        string ToastMessage = "";
        string ToastColor = "bg-green";
        string toastCss = "notification-hide";

        int rowCount = 0;
        bool AllSelected;
        bool isLoading;

        #endregion

        protected override async Task OnInitializedAsync()
        {
            if (!settings.IsLoggedIn)
            {
                nav.NavigateTo("/");
            }
            isLoading = true;
            await Task.Delay(1);
            await LoadCategories(); 
            isLoading = false;
            await Task.Delay(1);
        }

        #region Categories
        async Task LoadCategories()
        {

            Categories = await categoryService.GetAllFullCategories();
            UpdateSelectAllCheckboxes();
        }

        void CategoryCheckboxClicked(FullCategory cat)
        {
            isLoading = true;
            cat.AllSelected = !cat.AllSelected;
            foreach (var p in cat.Products)
            {
                p.Included = cat.AllSelected;
            }
            UpdateSelectAllCheckboxes();
            isLoading = false;
        }

        void OnHeadingSelect(IEnumerable<FullCategory> cat)
        {
            if (cat.Count() == 0) return;
            SelectedCategory = cat.First();
        }

        void SelectAll()
        {
            isLoading = true;
            AllSelected = !AllSelected;
            foreach (var cat in Categories)
            {
                foreach (var prod in cat.Products)
                {
                    prod.Included = AllSelected;
                }
            }
            UpdateSelectAllCheckboxes();
            isLoading = false;
        }


        void CategoryClicked(FullCategory fullCategory)
        {
            isLoading = true;
            if (OpenHeadings.Contains(fullCategory))
            {
                OpenHeadings.Remove(fullCategory);
            }
            else
            {
                OpenHeadings.Add(fullCategory);
            }
            isLoading = false;
        }

        async Task EditCategory(FullCategory fullCategory, int id)
        {
            var currState = CategoryGridRef.GetState();
            currState.EditItem = null;
            currState.OriginalEditItem = null;

            FullCategory itemToEdit = FullCategory.GetClonedInstance(fullCategory);

            currState.EditItem = itemToEdit;
            currState.OriginalEditItem = fullCategory;

            await CategoryGridRef.SetStateAsync(currState);

        }

        void CategoryUpdated(GridCommandEventArgs args)
        {
            FullCategory item = (FullCategory)args.Item;
            var selectedCat = Categories.Where(c => c.Id == item.Id).First();
            selectedCat.TariffCategory = item.TariffCategory;
        }
        #endregion

        #region Products

        async Task EditProduct(FullProduct prod)
        {
            var currState = ProductGridRef.GetState();
            currState.EditItem = null;
            currState.OriginalEditItem = null;

            FullProduct itemToEdit = FullProduct.GetClonedInstance(prod);

            currState.EditItem = itemToEdit;
            currState.OriginalEditItem = prod;

            await ProductGridRef.SetStateAsync(currState);

        }

        void ProductCheckboxClicked(FullProduct p)
        {
            isLoading = true;
            p.Included = !p.Included;

            UpdateSelectAllCheckboxes();
            isLoading = false;
        }

        void UpdateSelectAllCheckboxes()
        {
            foreach (var cat in Categories)
            {
                if (cat.Products.Count() == cat.Products.Where(p => p.Included == true).Count())
                {
                    cat.AllSelected = true;
                }
                else
                {
                    cat.AllSelected = false;
                }

            }

            if (Categories.Count() == Categories.Where(c => c.AllSelected == true).Count())
            {
                AllSelected = true;
            }
            else
            {
                AllSelected = false;
            }
        }

        async void SaveClicked()
        {
            isLoading = true;
            try
            {
                await categoryService.UpdateCategories(Categories);
                foreach (var c in Categories)
                {
                    await productService.UpdateProducts(c.Products);
                }
                await Task.Delay(1);
                isLoading = false;
                await Task.Delay(1);
                await InvokeAsync(() => StateHasChanged());

                await ShowToast("Succesfully Updated Categories", "bg-green");
            }
            catch
            {
                await ShowToast("Failed To Update Categories", "bg-red");
            }

        }

        void ProductUpdated(GridCommandEventArgs args)
        {
            FullProduct item = (FullProduct)args.Item;
            foreach (var c in Categories)
            {
                var selectedProd = c.Products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                if (selectedProd != null)
                {
                    selectedProd.ProductTariffName = item.ProductTariffName;
                }

            }

        }
        #endregion


        async Task ShowToast(string text, string color)
        {
            toastDisplayed = true;
            ToastMessage = text;
            toastCss = "notification-show";
            ToastColor = color;
            await InvokeAsync(() => StateHasChanged());
            await Task.Delay(3000);
            await HideToast();
        }

        async Task HideToast()
        {
            toastDisplayed = false;
            toastCss = "notification-hide";
            await InvokeAsync(() => StateHasChanged());
        }
    }
}
