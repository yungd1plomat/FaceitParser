using FaceitParser.Data;
using FaceitParser.Models;
using FaceitParser.Models.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace FaceitParser.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class BlacklistController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public BlacklistController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _context = dbContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Blacklist(string search = null, int page = 0)
        {
            search = search?.ToLower();
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            var blacklist = _context.Blacklists.Where(x => x.UserId == user.Id).ToList();
            if (blacklist is null)
                return View();
            if (search is not null)
            {
                blacklist = blacklist.Where(x => x.ProfileId.ToString() == search).ToList();
            }
            if (!blacklist.Any())
                return View();
            blacklist = blacklist.OrderBy(x => x.ProfileId).ToList();
            var chunked = blacklist.Chunk(14).ToList();
            if (page >= chunked.Count())
                return RedirectToAction("Blacklist", "Blacklist", new { search = search });
            return View(chunked[page].ToList());
        }

        // Принимаем string, т.к JS ебучий неправильно конвертирует числа
        [HttpPost("remove")]
        public async Task<IActionResult> Remove(string profile)
        {
            var profileId = ulong.Parse(profile);
            var user = await _userManager.GetUserAsync(User);
            var player = _context.Blacklists.FirstOrDefault(x => x.UserId == user.Id &&
                                                                                        x.ProfileId == profileId);
            if (player is null)
                return NotFound();
            _context.Blacklists.Remove(player);
            await _context.SaveChangesAsync();
            return Ok(profileId);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Add([FromForm] IFormFile blacklist)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            string? fileContents = null;
            using (var stream = blacklist.OpenReadStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    fileContents = await reader.ReadToEndAsync();
                }
            }

            if (fileContents is null)
                return NotFound();
            var lines = fileContents.Split(new char[] { '\n', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            var profileIds = Array.ConvertAll(lines, s => ulong.TryParse(s, out ulong x) ? x : ulong.MinValue).Distinct().ToList();
            profileIds.RemoveAll(x => x == ulong.MinValue);

            var userBlacklist = _context.Blacklists.Where(x => x.UserId == user.Id)?.ToList();
            var intersection = userBlacklist?.IntersectBy(profileIds, x => x.ProfileId)?.Select(x => x.ProfileId)?.ToList();
            profileIds?.RemoveAll(x => intersection?.Any(y => y == x) is true); // Удаляем все дубли в загружаемом списке

            var newBlacklist = profileIds?.Select(x => new BlacklistModel()
            {
                UserId = user.Id,
                ProfileId = x,
            });
            if (newBlacklist is not null &&
                newBlacklist.Any())
            {
                await _context.Blacklists.AddRangeAsync(newBlacklist);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost("clear")]
        public async Task<IActionResult> Clear()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            var blacklist = _context.Blacklists.Where(x => x.UserId == user.Id);
            if (blacklist.Any())
            {
                _context.Blacklists.RemoveRange(blacklist);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
