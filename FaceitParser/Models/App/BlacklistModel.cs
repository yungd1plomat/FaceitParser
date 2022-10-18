using System.ComponentModel.DataAnnotations;

namespace FaceitParser.Models
{
    public class BlacklistModel
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public List<Player> Players { get; set; }

        public BlacklistModel() { }

        public BlacklistModel(string userId, List<Player> players)
        {
            UserId = userId;
            Players = players;
        }
    }
}
