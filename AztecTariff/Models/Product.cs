namespace AztecTariff.Models
{
    public class Product
    {
        public long ProductId { get; set; }
        public int EstateId { get; set; }
        public string EntityCode { get; set; }
        public int Portion { get; set; }
        public string ProdName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool Deleted { get; set; }
        public double ABV { get; set; }
        public string TariffCategory { get; set; }
        public string ProductTariffName { get; set; }
        public bool Included { get; set; }

    }
}
