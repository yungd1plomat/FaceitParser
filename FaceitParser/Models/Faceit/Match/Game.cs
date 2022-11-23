using System.Text.Json.Serialization;

namespace FaceitParser.Models.Faceit
{
    public class Game
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
