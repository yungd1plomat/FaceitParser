using FaceitParser.Helpers;
using System.Text.Json.Serialization;

namespace FaceitParser.Models
{
    public class Inventory
    {
        [JsonPropertyName("descriptions")]
        public Description[] Items { get; set; }

        [JsonPropertyName("total_inventory_count")]
        public int Count { get; set; }

        [JsonPropertyName("success")]
        [JsonConverter(typeof(IntBoolConverter))]
        public bool Success { get; set; }
    }
}
