namespace AztecTariff.Models
{
    public class AztecSite
    {
        public int Id { get; set; }
        public int AreaId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string Manager { get; set; }
        public string TelephoneNumber { get; set; }
        public bool Deleted { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string AddressLine5 { get; set; }
        public string Postcode { get; set; }
    }
}
