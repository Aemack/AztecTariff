using AztecTariff.Models;
using AztecTariff.Services;
using AztecTariff.Shared;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using Telerik.Blazor;
using System.Diagnostics.Contracts;

namespace AztecTariff.Pages
{
    public partial class SiteTariff : BasePageClass
    {
        #region Props
        public FullProduct EditFullProduct { get; set; }
        public FullSalesArea EditSA { get; set; }

        TelerikGrid<FullSite> SiteGrid;
        TelerikGrid<FullCategory> CatGrid;
        TelerikGrid<FullProduct> ProdGrid;
        TelerikGrid<FullSalesArea> SAGrid;
        Toast Toast;


        FullSite SelectedSite = new FullSite();
        FullSalesArea SelectedSalesArea = new FullSalesArea();
        PDFMakerService pDFMaker;
        PricingService pricingService;

        SalesArea eventSAModel;
        FullEvent eventModel;

        List<string> templateChoices = new List<string>() { "Single-Page", "Multi-Page", "Landscape Multi-Page" };
        List<FullSite> Sites = new List<FullSite>();
        List<FullCategory> SelectedCategories = new List<FullCategory>();
        List<FullSite> SelectedSites = new List<FullSite>();

        CategoryService categoryService;
        SalesAreaService saService;

        byte[] PdfSource;
        string selectedTemplate;
        string docname;
        int rowCount;
        int subRowCount;
        bool includeAbv;
        bool isLoading;
        bool isAddEventModalVisible;
        bool isDeleteEventModalVisible;
        decimal basePriceMultiplier;
        #endregion
        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            selectedTemplate = templateChoices[0];
            saService = new SalesAreaService(DbFactory.CreateDbContext());
            categoryService = new CategoryService(DbFactory.CreateDbContext());
            await Task.Delay(1);
            pDFMaker = new PDFMakerService();
            pricingService = new PricingService(DbFactory.CreateDbContext());
            await LoadSites();
            isLoading = false;
        }



        #region Categories
        async Task CategoryMoved(GridRowDropEventArgs<FullCategory> args)
        {
            isLoading = true;
            await Task.Delay(1);
            if (args.Item != null)
            {
                var newIndex = (args.DropPosition == GridRowDropPosition.Before) ? int.Parse(args.DestinationIndex) - 1 : int.Parse(args.DestinationIndex);
                var oldIndex = SelectedSalesArea.Categories.IndexOf(args.Item);
                if (oldIndex > -1)
                {
                    SelectedSalesArea.Categories.RemoveAt(oldIndex);

                    if (newIndex > oldIndex) newIndex--;
                    // the actual index could have shifted due to the removal

                    SelectedSalesArea.Categories.Insert(newIndex, args.Item);
                    await UpdatePDF();
                }
            }
            isLoading = false;
            await Task.Delay(1);
        }

        void CategoryChevronClicked(FullCategory category)
        {
            if (SelectedCategories.Contains(category))
            {
                SelectedCategories.Remove(category);
            }
            else
            {
                SelectedCategories.Add(category);
            }
        }

        void UpdateAllSelected()
        {
            foreach (var cat in SelectedSalesArea.Categories)
            {
                if (cat.IncludedInPDFProducts.Count == cat.IncludedProducts.Count)
                {
                    cat.AllSelected = true;
                }
                else
                {
                    cat.AllSelected = false;
                }
            }
        }

        async Task CategoryCheckboxClicked(FullCategory category)
        {
            isLoading = true;
            await Task.Delay(1);
            //category.AllSelected = !category.AllSelected;
            foreach (var p in category.Products)
            {
                p.IncludeInPDF = category.AllSelected;
            }
            UpdateAllSelected();
            await UpdatePDF();
            isLoading = false;
            await Task.Delay(1);
        }
        #endregion

        #region SalesAreas
        async Task LoadSites()
        {
            Sites = await saService.GetAllFullSites();
        }

        void SiteMoved(GridRowDropEventArgs<FullSite> args)
        {
            if (args.Item != null)
            {
                var newIndex = (args.DropPosition == GridRowDropPosition.Before) ? int.Parse(args.DestinationIndex) - 1 : int.Parse(args.DestinationIndex);
                var oldIndex = Sites.IndexOf(args.Item);
                if (oldIndex > -1)
                {
                    Sites.RemoveAt(oldIndex);

                    if (newIndex > oldIndex) newIndex--;
                    // the actual index could have shifted due to the removal

                    Sites.Insert(newIndex, args.Item);
                }
            }
        }

        void SiteChevronClicked(FullSite site)
        {
            isLoading = true;
            if (SelectedSites.Contains(site))
            {
                SelectedSites.Remove(site);
            }
            else
            {
                SelectedSites.Add(site);
            }
            isLoading = false;
        }

        async Task SalesAreaSelected(FullSalesArea salesArea)
        {
            isLoading = true;
            await Task.Delay(1);
            SelectedSalesArea = salesArea;
            UpdateAllSelected();
            await UpdatePDF();
            isLoading = false;
            await Task.Delay(1);
        }

        async Task SaveSalesArea()
        {
            isLoading = true;

            await ExitEditSAAsync();

            var foundItem = await saService.GetSalesArea(EditSA.SalesAreaId);
            try
            {

                foundItem.TariffName = EditSA.TariffName;
                foundItem.FooterMessage = EditSA.FooterMessage;

                await saService.UpdateSalesArea(foundItem);
                await Task.Delay(1);
                await InvokeAsync(() => StateHasChanged());

                await Toast.DisplayMessage("Succesfully Updated Sales Area Price", "bg-green");
                var x = await saService.GetSalesArea(foundItem.SalesAreaId);


                await LoadSites();
                UpdateAllSelected();
                SelectedSalesArea = Sites.Select(s => s.SalesAreas.Where(sa => sa.SalesAreaId == SelectedSalesArea.SalesAreaId).First()).First();

                await UpdatePDF();
            }
            catch (Exception ex)
            {
                await Toast.DisplayMessage("Failed To Update Sales Area Price", "bg-red");
            }
            isLoading = false;
        }

        async Task ExitEditSAAsync()
        {
            var state = SAGrid?.GetState();
            state.OriginalEditItem = null;
            state.EditItem = null;
            state.InsertedItem = null;

            await SAGrid?.SetStateAsync(state);
        }

        async Task EditSalesArea(FullSalesArea sa)
        {
            var currState = SAGrid.GetState();
            currState.EditItem = null;
            currState.OriginalEditItem = null;

            FullSalesArea itemToEdit = FullSalesArea.GetClonedInstance(sa);

            currState.EditItem = itemToEdit;
            currState.OriginalEditItem = sa;

            await SAGrid.SetStateAsync(currState);
        }

        async Task AddEventPricing(FullSalesArea fsa)
        {
            var salesarea = await saService.GetSalesArea(fsa.SalesAreaId);
            var sa = new SalesArea()
            {
                SiteName = salesarea.SiteName,
                SiteId = salesarea.SiteId,
                isEvent = true,
                EstateId = salesarea.EstateId,
                SAName = $"{salesarea.SAName} Event",
                Included = true,
                OriginalSalesAreaId = salesarea.SalesAreaId,
            };
            basePriceMultiplier = 1;
            eventSAModel = sa;
            isAddEventModalVisible = true;

        }

        async Task SaveEventPricing()
        {

            isAddEventModalVisible = false;
            isLoading = true;
            await Task.Delay(1);
            var sa = eventSAModel;
            int newSaId = await saService.AddSalesArea(eventSAModel);
            //Make new pricing records for all the products
            var products = pricingService.GetPricingBySA(sa.OriginalSalesAreaId);
            foreach(var p in products)
            {
                await pricingService.AddPricing(new Pricing()
                {
                    EstateId = sa.EstateId,
                    ProductId = p.ProductId,
                    SalesAreaId = newSaId,
                    Price = p.Price * basePriceMultiplier,
                });
            }
            var allSAs = Sites.SelectMany(x => x.SalesAreas).ToList();
            var selectedSA = allSAs.Where(y => y.SalesAreaId == eventSAModel.OriginalSalesAreaId).First();
            

            var createdEvent = await saService.GetSalesArea(newSaId);
            var newEvent = await saService.ToFullEvent(createdEvent);
            selectedSA.Events.Add(newEvent);


            SelectedSalesArea = newEvent;
            await LoadSites();
            UpdateAllSelected();
            await UpdatePDF();

            isLoading = false;
            await Task.Delay(1);
        }

        void RemoveEventPricing(FullEvent fe)
        {
            eventModel = fe;
            isDeleteEventModalVisible = true;   
        }

        async Task ConfirmRemoveEventPricing(FullSalesArea fe)
        {
            isLoading = true;
            await saService.DeleteSalesArea(fe.SalesAreaId);
            await pricingService.DeletePricingBySA(fe.SalesAreaId);
            await LoadSites();
            isLoading = false;
            isDeleteEventModalVisible = false;
        }

        void CloseModal()
        {
            isDeleteEventModalVisible = false;
            isAddEventModalVisible = false;
        }
        #endregion

        #region Products
        async Task ProductMoved(GridRowDropEventArgs<FullProduct> args)
        {
            throw new NotImplementedException();
        }

        async Task ProductCheckboxClicked(FullProduct product)
        {
            isLoading = true;
            await Task.Delay(1);
            product.IncludeInPDF = !product.IncludeInPDF;
            UpdateAllSelected();
            await UpdatePDF();
            isLoading = false;
            await Task.Delay(1);
        }

        async Task SaveProduct()
        {
            isLoading = true;

            await ExitEditAsync();

            var foundItem = await pricingService.GetProductPricing(EditFullProduct.ProductId, SelectedSalesArea.SalesAreaId);
            try
            {
                var x = SelectedSalesArea.Categories.Select(x => x.Products.Where(p => p.ProductId == foundItem.ProductId).ToList());
                var gridItem = x.First().First();
                //var gridItem = SelectedSalesArea.Categories.Select(x => x.Products.Where(p => p.ProductId == foundItem.ProductId).First()).First();
                foundItem.Price = EditFullProduct.Price;
                gridItem.Price = EditFullProduct.Price;
                await pricingService.UpdatePricing(foundItem);
                await Task.Delay(1);
                await Task.Delay(1);
                await InvokeAsync(() => StateHasChanged());

                await Toast.DisplayMessage("Succesfully Updated Sales Area Price", "bg-green");

            }
            catch (Exception ex)
            {
                await Toast.DisplayMessage("Failed To Update Sales Area Price", "bg-red");
            }

            await UpdatePDF();
            isLoading = false;
        }

        async Task EditProduct(FullProduct product)
        {
            var currState = ProdGrid.GetState();
            currState.EditItem = null;
            currState.OriginalEditItem = null;

            FullProduct itemToEdit = FullProduct.GetClonedInstance(product);

            currState.EditItem = itemToEdit;
            currState.OriginalEditItem = product;

            await ProdGrid.SetStateAsync(currState);

        }

        void OnValidSubmit()
        {
            Console.WriteLine();
        }

        async Task ExitEditAsync()
        {
            var state = ProdGrid?.GetState();
            state.OriginalEditItem = null;
            state.EditItem = null;
            state.InsertedItem = null;

            await ProdGrid?.SetStateAsync(state);
        }

        async Task CancelEditProduct()
        {
            await ExitEditAsync();
        }
        #endregion

        #region PDF
        async Task UpdatePDF()
        {
            if (SelectedSalesArea.Categories.Sum(x => x.LinesRequired) < 5) return;



            docname = await pDFMaker.MakePdf(SelectedSalesArea, selectedTemplate, includeAbv);
            if (string.IsNullOrWhiteSpace(docname)) return;
            Byte[] fileBytes = File.ReadAllBytes(@$"{docname}");
            var content = Convert.ToBase64String(fileBytes);
            if (PdfSource == Convert.FromBase64String(content))
            {
                Console.WriteLine("Same thing");
            }
            PdfSource = Convert.FromBase64String(content);
            await Task.Delay(1);
        }

        async Task TemplateChanged()
        {
            isLoading = true;
            await Task.Delay(1);
            Console.WriteLine(selectedTemplate);
            await UpdatePDF();
            isLoading = false;
            await Task.Delay(1);

        }

        async Task IncludeABVChanged()
        {
            isLoading = true;
            await Task.Delay(1);
            Console.WriteLine(selectedTemplate);
            await UpdatePDF();
            isLoading = false;
            await Task.Delay(1);
        }
        #endregion



    }
}
