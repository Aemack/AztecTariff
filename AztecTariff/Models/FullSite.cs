namespace AztecTariff.Models
{
    public class FullSite
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public List<FullSalesArea> SalesAreas { get; set; }
        public bool Included { get; set; }
    }
}
