using System.Text.Json.Serialization;

namespace FaceitParser.Models.Faceit
{
    public class StatsPlayer
    {
        [JsonPropertyName("playerId")]
        public string Id { get; set; }
    }
}
