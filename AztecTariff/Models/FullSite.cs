namespace AztecTariff.Models
{
    public class FullSite
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<FullCategory>? Categories { get; set; } = new List<FullCategory>();

        public static FullSite GetClonedInstance(FullSite itmToClone)
        {
            var fs = new FullSite()
            {
                Name = itmToClone.Name,
                Categories = new List<FullCategory>()
            };

            foreach(var c in itmToClone.Categories)
            {
                fs.Categories.Add(FullCategory.GetClonedInstance(c));
            }
            return fs;
        }
    }
}
