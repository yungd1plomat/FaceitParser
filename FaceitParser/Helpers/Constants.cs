using FaceitParser.Models;

namespace FaceitParser.Helpers
{
    public static class Constants
    {
        public static string[] ProxyTypes = { "https", "socks4", "socks5" };

        public static IEnumerable<Location> Locations = new List<Location>()
        {
            new Location("EU", "42e160fc-2651-4fa5-9a9b-829199e27adb", null, new string[] { "ru", "ua", "az", "am", "by", "kz", "kg", "tj", "tm", "uz", "ua"}),
            new Location("RU (sng)", "42e160fc-2651-4fa5-9a9b-829199e27adb", new string[] { "ru", "ua", "az", "am", "by", "kz", "kg", "tj", "tm", "uz", "ua"}),
            new Location("EU prem", "a3c75828-7f0f-4940-adb9-994b4b389070", null, new string[] { "ru", "ua", "az", "am", "by", "kz", "kg", "tj", "tm", "uz", "ua"}),
            new Location("RU (sng) prem", "a3c75828-7f0f-4940-adb9-994b4b389070", new string[] { "ru", "ua", "az", "am", "by", "kz", "kg", "tj", "tm", "uz", "ua"}),
            new Location("Oceania", "0169b864-b045-4248-b648-f583bdbe6644"),
            new Location("SEA", "b3a17756-4d3a-4d53-9820-d31593c7a45f"),
            new Location("US", "002d828e-cbd9-4ad2-acf0-09d759eb1d09"),
            new Location("SA", "1c488dc9-4026-4a27-b49b-9204fe5d550f"),
        };
    }
}
