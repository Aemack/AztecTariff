namespace AztecTariff.Models
{
    public class FullSalesArea
    {
        public int SalesAreaId { get; set; }
        public List<FullCategory> Categories { get; set; }
        public string SalesAreaName { get; set; }
        public string TariffName { get; set; }
        public bool Included { get; set; }
        public string FooterMessage { get; set; } = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce et elementum mauris, vel viverra tortor";
    }
}
