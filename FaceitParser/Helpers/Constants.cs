using FaceitParser.Models;

namespace FaceitParser.Helpers
{
    public static class Constants
    {
        public static string[] ProxyTypes = { "https", "socks4", "socks5" };

        public static IEnumerable<Location> Locations = new List<Location>()
        {
            new Location("EU", "EU", null, new string[] { "ru", "ua", "az", "am", "by", "kz", "kg", "tj", "tm", "uz", "ua"}),
            new Location("RU (sng)", "EU", new string[] { "ru", "ua", "az", "am", "by", "kz", "kg", "tj", "tm", "uz", "ua"}),
            new Location("Oceania", "Oceania"),
        };
    }
}
