using FaceitParser.Helpers;
using System.Text.Json.Serialization;

namespace FaceitParser.Models
{
    public class Player
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("nickname")]
        public string Nick { get; set; }

        [JsonPropertyName("gameId")]
        [JsonConverter(typeof(StringToLongConverter))]
        public ulong ProfileId { get; set; }

        [JsonPropertyName("gameSkillLevel")]
        public int Level { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        public int? Matches { get; set; }
    }
}
