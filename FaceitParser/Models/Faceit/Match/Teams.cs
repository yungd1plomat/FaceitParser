using System.Text.Json.Serialization;

namespace FaceitParser.Models.Faceit
{
    public class Teams
    {
        [JsonPropertyName("faction1")]
        public TeamsFaction1 Faction1 { get; set; }

        [JsonPropertyName("faction2")]
        public TeamsFaction1 Faction2 { get; set; }
    }

}
