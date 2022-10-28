using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace FaceitParser.Models
{
    public class FaceitAccount
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }

        public string UserId { get; set; }
    }
}
