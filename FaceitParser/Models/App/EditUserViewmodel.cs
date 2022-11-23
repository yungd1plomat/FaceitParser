using System.ComponentModel.DataAnnotations;

namespace FaceitParser.Models.App
{
    public class EditUserViewmodel
    {
        [Required]
        public string OldUsername { get; set; }

        public string? NewUsername { get; set; }

        public string? Password { get; set; }

        public string? Role { get; set; }
    }
}
