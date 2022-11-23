using System.Text.Json.Serialization;

namespace FaceitParser.Models.Faceit
{
    public class Match
    {
        [JsonPropertyName("teams")]
        public Teams Teams { get; set; }
    }
}
