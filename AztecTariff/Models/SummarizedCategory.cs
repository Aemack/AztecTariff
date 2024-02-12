namespace AztecTariff.Models
{
    public class SummarizedCategory
    {
        public int Id { get; set; }
        public int SalesAreaID { get; set; }
        public string Category { get; set; }
        public int CategoryId { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public string SummaryDescription { get; set; }
    }
}
