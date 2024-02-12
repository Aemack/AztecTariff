namespace AztecTariff.Models
{
    public class PDFProduct
    {
        public int Id { get; set; }
        public int PDFDataId { get; set; }
        public long ProductID { get; set; }
        public decimal Pricing { get; set; }
        public bool IncludedInPdf { get; set; }
        public string DisplayName { get; set; }


    }



}
