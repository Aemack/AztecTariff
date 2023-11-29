namespace AztecTariff.Models
{
    //Product to get sent to pdf 
    public class FullProduct
    {
        public long ProductId { get; set; }
        public string ProductTariffName { get; set; }
        public string ProdName { get; set; }
        public int Portion { get; set; }
        public decimal Price { get; set; }
        public double ABV { get; set; }
        public bool Included { get; set; }
        public bool IncludeInPDF { get; set; }
        public static FullProduct GetClonedInstance(FullProduct itmToClone)
        {
            return new FullProduct()
            {
                ABV = itmToClone.ABV,
                Included = itmToClone.Included,
                Portion = itmToClone.Portion,
                Price = itmToClone.Price,
                ProdName = itmToClone.ProdName,
                ProductId = itmToClone.ProductId,
                ProductTariffName = itmToClone.ProductTariffName,
            };
        }
    }
}
