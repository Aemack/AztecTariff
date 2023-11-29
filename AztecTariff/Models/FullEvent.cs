namespace AztecTariff.Models
{
    public class FullEvent : FullSalesArea
    {
        public int OriginalSalesAreaId { get; set; }


        public static FullEvent GetClonedInstance(FullEvent fe)
        {
            FullEvent clone = new FullEvent
            {
                SalesAreaId = fe.SalesAreaId,
                Categories = fe.Categories,
                SalesAreaName = fe.SalesAreaName,
                TariffName = fe.TariffName,
                Included = fe.Included,
                FooterMessage = fe.FooterMessage,
                OriginalSalesAreaId = fe.OriginalSalesAreaId
            };

            return clone;
        }

    }
}
