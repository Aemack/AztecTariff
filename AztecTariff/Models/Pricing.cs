namespace AztecTariff.Models
{
    public class Pricing
    {
        public int Id { get; set; }
        public int SalesAreaId { get; set; }
        public int EstateId { get; set; }
        public long ProductId { get; set; }
        public decimal Price { get; set; }
    }
}
