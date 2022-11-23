using System.Text.Json.Serialization;

namespace FaceitParser.Models.Faceit
{
    public class PlayerIds
    {
        [JsonPropertyName("ids")]
        public IEnumerable<string> Ids { get; set; }
    }
}
