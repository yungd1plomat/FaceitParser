namespace FaceitParser.Models.App
{
    public class Location
    {
        public string Name { get; set; }

        public string RegionId { get; set; }

        public IEnumerable<string> Countries { get; set; }

        public IEnumerable<string> IgnoreCountries { get; set; }

        public Location(string name, string regionId, IEnumerable<string> countries = null, IEnumerable<string> ignoreCountries = null)
        {
            Name = name;
            RegionId = regionId;
            Countries = countries;
            IgnoreCountries = ignoreCountries;
        }
    }
}
