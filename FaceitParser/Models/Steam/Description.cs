using FaceitParser.Helpers;
using System.Text.Json.Serialization;

namespace FaceitParser.Models
{
    public class Description
    {
        [JsonPropertyName("tradable")]
        [JsonConverter(typeof(IntBoolConverter))]
        public bool Tradable { get; set; }

        [JsonPropertyName("market_name")]
        public string Name { get; set; }
    }
}
