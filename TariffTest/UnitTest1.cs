using AztecTariff.Data;
using AztecTariff.Models;
using AztecTariff.Services;
using Microsoft.EntityFrameworkCore;

namespace TariffTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task TestPDFGenerator()
        {
            Settings s = new Settings()
            {
                CSVFileLocation = "C:\\Users\\AdamM2\\OneDrive - Zonal Retail Data Systems Limited\\Desktop\\TariffNotesNStuff\\New folder",
                LibreLocation = "C:\\Users\\AdamM2\\Downloads\\LibreOfficePortable\\LibreOfficePortable.exe",
                TemplateFolderLocation = "C:\\Users\\AdamM2\\OneDrive - Zonal Retail Data Systems Limited\\Desktop\\TariffNotesNStuff",
                WordFileLocation = "C:\\Users\\AdamM2\\OneDrive - Zonal Retail Data Systems Limited\\Desktop\\TariffNotesNStuff\\New folder\\Test.docx",
            };
            var optionsBuilder = new DbContextOptionsBuilder<TariffDatabaseContext>();
            optionsBuilder.UseSqlite();
            var _dbContext = new TariffDatabaseContext(optionsBuilder.Options);

            FullSalesArea fullSalesArea = FileWritingService.LoadFromXml<FullSalesArea>("C:\\Users\\AdamM2\\Downloads\\TestSalesArea.xml");
            PDFMakerService pdfMakerService = new PDFMakerService(s);
            
            var filePath = await pdfMakerService.MakePdf(fullSalesArea, "Single-Page", false);

            Assert.That(File.Exists(filePath));
        }
    }
}