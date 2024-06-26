﻿using System.ComponentModel.DataAnnotations;

namespace FaceitParser.Models.App
{
    public class CreateParserVm
    {
        [RegularExpression("[A-Za-z0-9]+", ErrorMessage = "Название парсера должно состоять из латинских букв и цифр")]
        [Required]
        public string Name { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Допустимый лвл 1-10")]
        public int MaxLvl { get; set; }

        [Required]
        [Range(1000, int.MaxValue, ErrorMessage = $"Минимальная допустимая задержка 1000 мс")]
        public int Delay { get; set; }


        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Минимальная сумма инвентаря 0 $")]
        public int MinPrice { get; set; }


        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Минимальное значение для максимального количество матчей 0")]
        public int MaxMatches { get; set; }

        [Required]
        public bool AutoAdd { get; set; }


        [RegularExpression(@"(\d{1,3}\.){3}\d{1,3}:(\d+):(.*?):(.*)", ErrorMessage = "Неверный формат прокси")]
        public string? Proxy { get; set; }

        public string? ProxyType { get; set; }
    }
}
