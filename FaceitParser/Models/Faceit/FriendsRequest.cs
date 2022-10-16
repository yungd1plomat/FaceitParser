using System.Text.Json.Serialization;

namespace FaceitParser.Models
{
    public class FriendsRequest
    {
        [JsonPropertyName("users")]
        public List<string> Users { get; set; }
    }
}
