using Microsoft.AspNetCore.Identity;

namespace FaceitParser.Data
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public DateTimeOffset CreateDate { get; set; }
    }
}
