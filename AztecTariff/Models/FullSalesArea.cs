namespace AztecTariff.Models
{
    public class FullSalesArea
    {
        public int SalesAreaId { get; set; }
        public List<FullCategory> Categories { get; set; }
        public string SalesAreaName { get; set; }
        public string TariffName { get; set; }
        public bool Included { get; set; }
    }
}
