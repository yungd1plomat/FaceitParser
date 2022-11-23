using System.Text.Json.Serialization;

namespace FaceitParser.Models.Faceit
{
    public class PlayerInfo
    {
        [JsonPropertyName("payload")]
        public Dictionary<string, Player> Players { get; set; }
    }
}
