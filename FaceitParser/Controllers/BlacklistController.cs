using FaceitParser.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FaceitParser.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class BlacklistController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<IdentityUser> _userManager;

        public BlacklistController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _context = dbContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Blacklist(string search = null, int page = 0)
        {
            search = search?.ToLower();
            var user = await _userManager.GetUserAsync(User);
            var userBlacklist = _context.Blacklists.Include(x => x.Players).FirstOrDefault(x => x.UserId == user.Id);
            if (userBlacklist is null)
                return View();
            var players = userBlacklist.Players;
            if (search is not null)
            {
                players = players.Where(x => x.Nick.ToLower().Contains(search) ||
                                             x.Level.ToString() == search ||
                                             x.Country.ToLower() == search).ToList();
            }
            if (!players.Any())
                return View();
            players = players.OrderBy(x => x.Nick).ToList();
            var chunked = players.Chunk(14).ToList();
            if (page >= chunked.Count())
                return RedirectToAction("Blacklist", "Blacklist", new { search = search });
            userBlacklist.Players = chunked[page].ToList();
            return View(userBlacklist);
        }

        // Принимаем string, т.к JS ебучий неправильно конвертирует числа
        [HttpPost("remove")]
        public async Task<IActionResult> Remove(string profile)
        {
            var profileId = ulong.Parse(profile);
            var user = await _userManager.GetUserAsync(User);
            var userBlacklist = _context.Blacklists.Include(x => x.Players).FirstOrDefault(x => x.UserId == user.Id);
            if (userBlacklist is null)
                return NotFound();
            var player = userBlacklist.Players.FirstOrDefault(x => x.ProfileId.Equals(profileId));
            if (player is null)
                return NotFound();
            userBlacklist.Players.Remove(player);
            await _context.SaveChangesAsync();
            return Ok(profileId);
        }
    }
}
