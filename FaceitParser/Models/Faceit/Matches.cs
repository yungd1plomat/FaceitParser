using System.Text.Json.Serialization;

namespace FaceitParser.Models
{
    public class Matches
    {
        [JsonPropertyName("payload")]
        public Game[] Games { get; set; }
    }
}
