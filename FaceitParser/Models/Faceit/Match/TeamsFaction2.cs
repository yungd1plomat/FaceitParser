using System.Text.Json.Serialization;

namespace FaceitParser.Models
{
    public class TeamsFaction2
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("roster")]
        public Player[] Players { get; set; }
    }
}
