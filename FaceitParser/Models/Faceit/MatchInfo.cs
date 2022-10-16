using System.Text.Json.Serialization;

namespace FaceitParser.Models
{
    public class MatchInfo
    {
        [JsonPropertyName("payload")]
        public Match Match { get; set; }
    }
}
