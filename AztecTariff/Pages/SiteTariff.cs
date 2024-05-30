using AztecTariff.Models;
using AztecTariff.Services;
using AztecTariff.Shared;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using Telerik.Blazor;
using System.Diagnostics.Contracts;
using System.Xml.Serialization;
using Microsoft.JSInterop;
using System.IO.Compression;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using AztecTariffModels.Models;

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


        SalesArea eventSAModel;
        FullEvent eventModel;

        SummarizedCategory summaryModel;
        List<string> templateChoices = new List<string>() { "Single-Page", "Multi-Page", "Landscape Multi-Page" };
        List<FullSite> Sites = new List<FullSite>();
        List<FullCategory> SelectedCategories = new List<FullCategory>();
        List<FullSite> SelectedSites = new List<FullSite>();

        List<FullPDFData> ExportableSalesAreas = new List<FullPDFData>();
        List<FullPDFData> SelectedSalesAreas = new List<FullPDFData>();

        DateTime selectedDateValue = DateTime.Today;

        bool sitesCollapsed;
        bool categoriesCollapsed;
        bool isAddSummaryModalVisible;
        string gridClass => (sitesCollapsed && categoriesCollapsed) ? "row p-0 " : "row p-0 grid-container";
        string siteTableClass => (sitesCollapsed) ? "grid-col-collapsed" : "grid-col";
        //string siteTableClass => (sitesCollapsed) ? "grid-col-collapsed p-0" : "grid-col col p-0";
        string categoryTableClass => (categoriesCollapsed) ? "grid-col-collapsed" : "grid-col";
        //string categoryTableClass => (categoriesCollapsed) ? "grid-col-collapsed p-0" : "grid-col col p-0";
        string sitesButtonClass => (sitesCollapsed) ? "btn overlay-button-1" : "d-none";
        string categoryButtonClass => (categoriesCollapsed) ? "btn overlay-button-1" : "d-none";

        byte[] PdfSource;
        string selectedTemplate;
        string docname;
        int rowCount;
        int subRowCount;
        bool includeAbv;
        bool isLoading;
        bool isAddEventModalVisible;
        bool isMultiExportVisible;
        bool isMultiPrintVisible;
        bool isDeleteEventModalVisible;
        double basePriceMultiplier;
        #endregion


        protected override async Task OnInitializedAsync()
        {
            if (!settings.IsLoggedIn)
            {
                nav.NavigateTo("/");
                return;
            }
            isLoading = true;
            selectedTemplate = templateChoices[0];
            await Task.Delay(1);
            pDFMaker = new PDFMakerService(settings);

            await LoadSites();
            //if (Sites.Count > 0 && Sites[0].SalesAreas.Count > 0)
            //{
            //    SelectedSalesArea = Sites.FirstOrDefault().SalesAreas.FirstOrDefault();
            //    SalesAreaSelected(SelectedSalesArea);
            //}
            UpdateAllSelected();
            
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
            if (SelectedSalesArea.TariffName == null)
            {
                return;
            }
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


        public async void SummarizeCategoryToggle(FullCategory category)
        {
            if (category.IsSummarized)
            {
                isLoading = true;
                isAddSummaryModalVisible = false;
                category.SummarizedCategory = null;
                await catService.DeleteSummarizedCategory(category, SelectedSalesArea.SalesAreaId);
                await UpdatePDF();
                isLoading = false;
                return;
            }


            summaryModel = new SummarizedCategory();
            summaryModel.Category = category.TariffCategory;
            summaryModel.SummaryDescription = "Prices range from";
            summaryModel.SalesAreaID = SelectedSalesArea.SalesAreaId;
            summaryModel.MinPrice = category.IncludedProducts.Select(p => p.Price).Min();
            summaryModel.MaxPrice = category.IncludedProducts.Select(p => p.Price).Max();
            

            isAddSummaryModalVisible = true;
        }

        async void SaveCategorySummary()
        {
            isLoading = true;
            isAddSummaryModalVisible = false;
            await Task.Delay(1);

            summaryModel.CategoryId = catService.GetCategoryId(summaryModel.Category);
            await catService.UpdateSummarizedCategory(summaryModel);
            
            var x = SelectedSalesArea.Categories.Where(x => x.TariffCategory == summaryModel.Category).FirstOrDefault();
            if(x != null)
            {
                x.SummarizedCategory = summaryModel;
            }
            
            await UpdatePDF();

            isLoading = false;
            await Task.Delay(1);
            StateHasChanged();


        }
        #endregion

        #region SalesAreas
        async Task LoadSites()
        {
            Sites = await saService.GetAllFullSitesByDate(selectedDateValue);
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
                foundItem.HeaderMessage = EditSA.HeaderMessage;

                await saService.UpdateSalesArea(foundItem);
                await Task.Delay(1);
                await InvokeAsync(() => StateHasChanged());

                await Toast.DisplayMessage("Succesfully Updated Sales Area Price", "bg-green");
                var x = await saService.GetSalesArea(foundItem.SalesAreaId);


                await LoadSites();
                //UpdateAllSelected();
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
            foreach (var p in products)
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
            //UpdateAllSelected();
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

        void MinimiseSites()
        {
            sitesCollapsed = !sitesCollapsed;
        }

        void MinimiseCategories()
        {
            categoriesCollapsed = !categoriesCollapsed;
        }

        async Task SavePDFData(string docname)
        {
            if (SelectedSalesArea.TariffName != null)
            {
                try
                {
                    var x = new PDFData();
                    x.TempFileName = docname;
                    x.SalesAreaID = SelectedSalesArea.SalesAreaId;
                    x.IncludeABV = includeAbv;
                    x.Template = selectedTemplate;
                    x.CreatedDate = DateTime.Now;

                    var pdfDataId = await pdfdataservice.UpdatePDFData(x);

                    foreach (var y in SelectedSalesArea.Categories)
                    {
                        foreach (var c in y.IncludedProducts)
                        {
                            var pp = new PDFProduct()
                            {
                                ProductID = c.ProductId,
                                DisplayName = c.ProductTariffName,
                                PDFDataId = pdfDataId,
                                IncludedInPdf = c.IncludeInPDF,
                                Pricing = c.Price,
                            };
                            await pdfdataservice.UpdatePDFProduct(pp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync("Something went wrong!");
                }
            }
        }

        void GetPrintableSites()
        {
            ExportableSalesAreas = pdfdataservice.GetAllFullPDFData();

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
                foundItem.Price = (double)EditFullProduct.Price;
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

        async Task DateChanged()
        {
            isLoading = true;
            await LoadSites();
            //Look at just updating the prices rather than getting all the sites again
            isLoading = false;
        }
        #endregion

        #region PDF
        async Task UpdatePDF()
        {
            if (SelectedSalesArea.Categories.Sum(x => x.LinesRequired) < 5) return;
            docname = pDFMaker.MakePDF(SelectedSalesArea, selectedTemplate, includeAbv);
            if (string.IsNullOrWhiteSpace(docname)) return;
            Byte[] fileBytes = File.ReadAllBytes(@$"{docname}");
            var content = Convert.ToBase64String(fileBytes);
            if (PdfSource == Convert.FromBase64String(content))
            {
                Console.WriteLine("Same thing");
            }
            PdfSource = Convert.FromBase64String(content);
            await Task.Delay(1);

            await SavePDFData(docname);
            isLoading = false;
        }

        async Task TemplateChanged()
        {
            isLoading = true;
            await Task.Delay(1);
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

        async Task ExportMultipleClicked()
        {
            isMultiExportVisible = true;
            GetAllPDFData();
        }

        async Task PrintMultipleClicked()
        {
            isMultiPrintVisible = true;
            GetAllPDFData();
        }

        public void GetAllPDFData()
        {
            ExportableSalesAreas = pdfdataservice.GetAllFullPDFData();
        }

        public async Task ExportFromMultiple()
        {
            var x = ExportableSalesAreas.Where(x => x.Selected).ToList();
            await CreateZipFile(x);
        }

        private async Task CreateZipFile(List<FullPDFData> x)
        {
            var filePaths = x.Select(x => x.PDFData.TempFileName).ToList();

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var filePath in filePaths)
                    {
                        var entry = archive.CreateEntry(Path.GetFileName(filePath));

                        using (var entryStream = entry.Open())
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }

                // Reset the stream's position to the beginning
                memoryStream.Seek(0, SeekOrigin.Begin);

                var mimeType = "application/zip";
                var fileName = "YourArchive.zip";

                var buffer = new byte[memoryStream.Length];
                await memoryStream.ReadAsync(buffer, 0, buffer.Length);

                var base64 = Convert.ToBase64String(buffer);
                var href = $"data:{mimeType};base64,{base64}";

                // Use NavigationManager to navigate to the dynamically generated data URL
                nav.NavigateTo(href, forceLoad: true);
            }


        }

        public async Task DownloadFiles(List<FullPDFData> x)
        {
            var fileUrls = new List<string>();
            foreach (var file in x)
            {
                fileUrls.Add(file.PDFData.TempFileName);
            }


            await JSRuntime.InvokeVoidAsync("downloadFiles", fileUrls);
        }

        public async Task PrintMultiplePDF()
        {
            var x = ExportableSalesAreas.Where(x => x.Selected).ToList();

            var filePaths = x.Select(x => x.PDFData.TempFileName).ToList();

            foreach (var filePath in filePaths)
            {

                var pdfFileName = filePath.Split(@"\").Last();
                //File.Copy(filePath, Path.Combine(WebHostEnvironment.ContentRootPath, "wwwroot", "temppdf", pdfFileName));

                var fullPath = Path.Combine(WebHostEnvironment.ContentRootPath, "wwwroot", "temppdf", pdfFileName);


                try
                {
                    var pdfContent = await File.ReadAllBytesAsync(filePath);
                    var base64Pdf = Convert.ToBase64String(pdfContent);
                    var pdfDataUri = $"data:application/pdf;base64,{base64Pdf}";
                    await JSRuntime.InvokeVoidAsync("printPdf", pdfDataUri);
                }
                catch (Exception ex)
                {
                    // Log or handle any exceptions
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            isMultiPrintVisible = false;
        }


        public static void SaveToXml(FullSalesArea obj, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FullSalesArea));

            using (FileStream fileStream = new FileStream(filename, FileMode.Create))
            {
                serializer.Serialize(fileStream, obj);
            }

            Console.WriteLine($"Object saved to {filename}");
        }
        #endregion
    }
}
