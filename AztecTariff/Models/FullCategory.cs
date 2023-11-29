using AztecTariff.Services;

namespace AztecTariff.Models
{
    public class FullCategory
    {
        public int Id { get; set; }
        public string TariffCategory { get; set; }
        public string CategoryName { get; set; }
        public List<FullProduct> Products { get; set; } = new List<FullProduct>();
        public List<FullProduct> IncludedProducts => Products.Where(p => p.Included).ToList();
        public List<FullProduct> IncludedInPDFProducts => IncludedProducts.Where(p => p.IncludeInPDF).ToList();
        public bool AllSelected { get; set; }
        public int LinesRequired => (IncludedInPDFProducts.Count > 0) ? IncludedInPDFProducts.Count + 1 : 0;
        public static FullCategory GetClonedInstance(FullCategory itmToClone)
        {
            return new FullCategory()
            {
                Id = itmToClone.Id,
                CategoryName = itmToClone.CategoryName,
                TariffCategory = itmToClone.TariffCategory,
                Products = itmToClone.Products
            };
        }
    }
}
