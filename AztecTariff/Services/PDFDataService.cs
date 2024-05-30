using AztecTariff.Models;
using AztecTariffModels.Models;
using System.Reflection.Metadata.Ecma335;

namespace AztecTariff.Services
{
    public class PDFDataService
    {
        private readonly TariffDatabaseContext _dbContext;
        Settings _settings;

        public PDFDataService(TariffDatabaseContext dbContext, Settings settings)
        {
            _dbContext = dbContext;
            _settings = settings;
        }

        public async Task<int> UpdatePDFData(PDFData pdfdata)
        {
            var foundEntry = _dbContext.PDFData.Where(x => x.SalesAreaID == pdfdata.SalesAreaID).FirstOrDefault();
            if (foundEntry != null)
            {
                //pdfdata.Id = foundEntry.Id;
                //_dbContext.Update(pdfdata);

                foundEntry.CreatedDate = DateTime.Now;
                foundEntry.Template = pdfdata.Template;
                foundEntry.TempFileName = pdfdata.TempFileName;
                foundEntry.IncludeABV = pdfdata.IncludeABV;
                foundEntry.SalesAreaID = pdfdata.SalesAreaID;
                _dbContext.Update(foundEntry);
            }
            else
            {
                _dbContext.Add(pdfdata);
            }
            await _dbContext.SaveChangesAsync();
            return pdfdata.Id;
        }

        public async Task<int> UpdatePDFProduct(PDFProduct pdfProd)
        {
            var foundEntry = _dbContext.PDFProducts.Where(x => x.ProductID == pdfProd.ProductID && x.PDFDataId == pdfProd.PDFDataId).FirstOrDefault();
            if (foundEntry != null)
            {
                //pdfProd.Id = foundEntry.Id;
                //_dbContext.Update(pdfProd);
                foundEntry.ProductID = pdfProd.ProductID;
                foundEntry.IncludedInPdf = pdfProd.IncludedInPdf;
                foundEntry.PDFDataId = pdfProd.PDFDataId;
                foundEntry.Pricing = pdfProd.Pricing;
                foundEntry.DisplayName = pdfProd.DisplayName;
                _dbContext.Update(foundEntry);
            }
            else
            {
                _dbContext.PDFProducts.Add(pdfProd);
            }
            await _dbContext.SaveChangesAsync();

            return (pdfProd.Id == 0) ? foundEntry.Id : pdfProd.Id;
        }

        public async Task UpdatePDFProducts(List<PDFProduct> pdfProducts)
        {
            foreach (var p in pdfProducts)
            {
                await UpdatePDFProduct(p);
            }
        }

        public List<PDFData> GetAllPDFData()
        {
            return _dbContext.PDFData.ToList();
        }

        public List<PDFProduct> GetAllProductData()
        {
            return _dbContext.PDFProducts.ToList();
        }

        public List<PDFProduct> GetProductDataBySite(int pdfDataId)
        {
            return _dbContext.PDFProducts.Where(x => x.PDFDataId == pdfDataId).ToList();
        }

        public async Task ClearPDFDataTable()
        {
            _dbContext.PDFData.RemoveRange(_dbContext.PDFData);
            await _dbContext.SaveChangesAsync();
        }

        public List<FullPDFData> GetAllFullPDFData()
        {
            var fullpdfdatas = new List<FullPDFData>();
            var pdfdata = GetAllPDFData();
            foreach (var p in pdfdata)
            {
                fullpdfdatas.Add(new FullPDFData()
                {
                    PDFData = p,
                    Products = GetProductDataBySite(p.Id),
                    SalesArea = _dbContext.SalesAreas.Where(x => x.SalesAreaId == p.SalesAreaID).FirstOrDefault()
                });
            }
            return fullpdfdatas;
        }

        public async Task ClearPDFProductTable()
        {
            _dbContext.PDFProducts.RemoveRange(_dbContext.PDFProducts);
            await _dbContext.SaveChangesAsync();
        }
    }
}