namespace FaceitParser.Models
{
    public class Location
    {
        public string Name { get; set; }

        public string Region { get; set; }

        public IEnumerable<string> Countries { get; set; }

        public IEnumerable<string> IgnoreCountries { get; set; }

        public Location(string name, string region, IEnumerable<string> countries = null, IEnumerable<string> ignoreCountries = null)
        {
            Name = name;
            Region = region;
            Countries = countries;
            IgnoreCountries = ignoreCountries;
        }
    }
}
