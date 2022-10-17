using System.ComponentModel.DataAnnotations;

namespace FaceitParser.Models.App
{
    public class CreateParserVm
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        [Range(2, 11, ErrorMessage = "Допустимый лвл 2-11")]
        public int MaxLvl { get; set; }

        [Required]
        [Range(350, int.MaxValue, ErrorMessage = $"Минимальная допустимая задержка 350 мс")]
        public int Delay { get; set; }


        [RegularExpression(@"(\d{1,3}\.){3}\d{1,3}:(\d+):(.*?):(.*)", ErrorMessage = "Неверный формат прокси")]
        public string? Proxy { get; set; }

        public string? ProxyType { get; set; }
    }
}
