using System.Text.Json.Serialization;

namespace FaceitParser.Models.Faceit
{
    public class PlayerStats
    {
        [JsonPropertyName("_id")]
        public StatsPlayer Player { get; set; }

        [JsonPropertyName("rev")]
        public int Matches { get; set; }
    }
}
