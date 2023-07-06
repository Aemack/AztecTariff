using AztecTariff.Services;

namespace AztecTariff.Models
{
    public class FullCategory
    {
        public int APIId { get; set; }
        public string Name { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Product> IncludedProducts => Products.Where(p => p.Included).ToList();
        public bool AllSelected { get; set; }
        public int LinesRequired => (IncludedProducts.Count > 0) ? IncludedProducts.Count + 1 : 0;
        public static FullCategory GetClonedInstance(FullCategory itmToClone)
        {
            return new FullCategory()
            {
                Name = itmToClone.Name,
                Products = itmToClone.Products
            };
        }
    }
}
