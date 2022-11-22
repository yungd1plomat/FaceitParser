using FaceitParser.Data;
using Microsoft.EntityFrameworkCore;

namespace FaceitParser.Helpers
{
    public static class ContextFactory
    {
        public static ApplicationDbContext CreateContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 27)));
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
