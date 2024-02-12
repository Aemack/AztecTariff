namespace AztecTariff.Models
{
    public class FullPDFData
    {
        public bool  Selected { get; set; }
        public PDFData PDFData { get; set; }
        public List<PDFProduct> Products { get; set; }
        public SalesArea SalesArea { get; set; }
        public List<SummarizedCategory> SummarizedCategories { get; set; }

    }
}
