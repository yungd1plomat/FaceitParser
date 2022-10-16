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
        public string ProfileId { get; set; }

        [JsonPropertyName("gameSkillLevel")]
        public int Level { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }
    }
}
