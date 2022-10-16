using System.Numerics;
using System.Text.Json.Serialization;

namespace FaceitParser.Models
{
    public class SelfData
    {
        [JsonPropertyName("payload")]
        public Player SelfPlayer { get; set; }
    }
}
