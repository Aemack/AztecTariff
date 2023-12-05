namespace AztecTariff.Models
{
    public class FullSalesArea
    {
        public int SalesAreaId { get; set; }
        public List<FullCategory> Categories { get; set; }
        public string SalesAreaName { get; set; }
        public string TariffName { get; set; }
        public string HeaderMessage { get; set; }
        public bool Included { get; set; }
        public string FooterMessage { get; set; } = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce et elementum mauris, vel viverra tortor";

        public List<FullEvent> Events { get; set; } = new List<FullEvent>();

        public static FullSalesArea GetClonedInstance(FullSalesArea sa)
        {
            FullSalesArea clone = new FullSalesArea
            {
                SalesAreaId = sa.SalesAreaId,
                Categories = sa.Categories,
                SalesAreaName = sa.SalesAreaName,
                TariffName = sa.TariffName,
                Included = sa.Included,
                FooterMessage = sa.FooterMessage,
                HeaderMessage = sa.HeaderMessage,
                Events = sa.Events
            };

            return clone;
        }
    }
}
