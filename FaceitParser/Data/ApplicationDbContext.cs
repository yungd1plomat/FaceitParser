using FaceitParser.Models;
using FaceitParser.Models.App;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FaceitParser.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<BlacklistModel> Blacklists { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}