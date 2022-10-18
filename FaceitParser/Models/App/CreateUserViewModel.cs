using System.ComponentModel.DataAnnotations;

namespace FaceitParser.Models
{
    public class CreateUserViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
