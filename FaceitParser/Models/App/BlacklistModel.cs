﻿using System.ComponentModel.DataAnnotations;

namespace FaceitParser.Models
{
    public class BlacklistModel
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        
        public ulong ProfileId { get; set; }
    }
}
