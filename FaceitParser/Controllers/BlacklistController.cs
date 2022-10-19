﻿using FaceitParser.Data;
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
            var blacklist = _context.Blacklists.Where(x => x.UserId == user.Id).ToList();
            if (blacklist is null)
                return View();
            if (search is not null)
            {
                blacklist = blacklist.Where(x => x.Nick.ToLower().Contains(search) ||
                                                 x.Level.ToString() == search ||
                                                 x.Country.ToLower() == search).ToList();
            }
            if (!blacklist.Any())
                return View();
            blacklist = blacklist.OrderBy(x => x.Nick).ToList();
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
    }
}