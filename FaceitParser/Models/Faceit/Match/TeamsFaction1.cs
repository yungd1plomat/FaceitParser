using System.Text.Json.Serialization;

namespace FaceitParser.Models.Faceit
{
    public class TeamsFaction1
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("roster")]
        public Player[] Players { get; set; }
    }
}
