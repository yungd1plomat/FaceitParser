using System.Text.Json.Serialization;

namespace FaceitParser.Models
{
    public class Match
    {
        [JsonPropertyName("teams")]
        public Teams Teams { get; set; }
    }
}
