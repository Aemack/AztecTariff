namespace AztecTariff.Models
{
    public class PDFData
    {
        public int Id { get; set; }
        public int SalesAreaID { get; set; }
        public string Template { get; set; }
        public string TempFileName { get; set; }
        public bool IncludeABV { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}


// For each SA, generate filename based on tariffname
//