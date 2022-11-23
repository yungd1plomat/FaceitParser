using System.Text.Json.Serialization;

namespace FaceitParser.Models
{
    public class PlayerInfo
    {
        [JsonPropertyName("payload")]
        public Dictionary<string, Player> Players { get; set; }
    }
}
