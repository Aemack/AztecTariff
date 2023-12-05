namespace AztecTariff.Models
{
    public class SalesArea
    {
        public int SalesAreaId { get; set; }
        public int EstateId { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string SAName { get; set; }
        public bool Deleted { get; set; }
        public bool Included { get; set; }
        public string TariffName { get; set; }
        public string FooterMessage { get; set; }
        public string HeaderMessage { get; set; }
        public bool isEvent { get; set; }
        public int? OriginalSalesAreaId { get; set; }
    }
}
