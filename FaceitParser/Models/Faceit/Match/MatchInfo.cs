using System.Text.Json.Serialization;

namespace FaceitParser.Models.Faceit
{
    public class MatchInfo
    {
        [JsonPropertyName("payload")]
        public Match Match { get; set; }
    }
}
