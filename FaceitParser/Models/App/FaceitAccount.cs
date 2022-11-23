using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace FaceitParser.Models.App
{
    public class FaceitAccount
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }

        public int FriendRequests { get; set; }

        public string UserId { get; set; }
    }
}
