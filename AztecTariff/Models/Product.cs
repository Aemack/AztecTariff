namespace AztecTariff.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int APIId { get; set; }
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public double? ABV { get; set; }
        public bool Included { get; set; }
    }
}
