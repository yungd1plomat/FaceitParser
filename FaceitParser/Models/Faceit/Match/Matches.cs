using System.Text.Json.Serialization;

namespace FaceitParser.Models.Faceit
{
    public class Matches
    {
        [JsonPropertyName("payload")]
        public Game[] Games { get; set; }
    }
}
