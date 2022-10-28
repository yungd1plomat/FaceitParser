using FaceitParser.Data;
using FaceitParser.Models;
using FaceitParser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FaceitParser.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class AccountsController : Controller
    {

        private readonly ApplicationDbContext _dbContext;

        private readonly UserManager<IdentityUser> _userManager;

        public AccountsController(ApplicationDbContext applicationDb, UserManager<IdentityUser> userManager)
        {
            _dbContext = applicationDb;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Accounts(string? search = null, int? page = 0, IEnumerable<string>? Errors = null)
        {
            if (Errors is not null)
            {
                foreach (var error in Errors)
                    ModelState.AddModelError(string.Empty, error);
            }
            var user = await _userManager.GetUserAsync(User);
            search = search?.ToLower();

            var userAccounts = search is null ? _dbContext.Accounts.Where(x => x.UserId == user.Id).ToList() :
                                                _dbContext.Accounts.Where(x => x.UserId == user.Id && (x.Name.ToLower()
                                                                                                            .Contains(search) ||
                                                                                                       x.Token.ToLower()
                                                                                                              .Contains(search))).ToList();

            if (!userAccounts.Any())
                return View(new List<FaceitAccount>());
            var chunked = userAccounts.Chunk(14).ToList();
            if (page >= chunked.Count())
                return RedirectToAction("Accounts", "Accounts");
            return View(chunked[page ?? 0].ToList());
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromForm] string Token)
        {
            if (_dbContext.Accounts.Any(x => x.Token == Token))
                ModelState.AddModelError(string.Empty, "Такой аккаунт уже добавлен!");
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                using (FaceitApi api = new FaceitApi(Token, CancellationToken.None))
                {
                    try
                    {
                        await api.GetSelf();
                        FaceitAccount account = new FaceitAccount()
                        {
                            Name = api.SelfNick,
                            FriendRequests = 0,
                            Token = Token,
                            UserId = user.Id,
                        };
                        await _dbContext.Accounts.AddAsync(account);
                        await _dbContext.SaveChangesAsync();
                    }
                    catch
                    {
                        ModelState.AddModelError(string.Empty, "Неверный токен");
                    }
                }
            }
            return RedirectToAction("Accounts", "Accounts", new { Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(string Token)
        {
            if (!_dbContext.Accounts.Any(x => x.Token == Token))
                return BadRequest();
            var account = _dbContext.Accounts.FirstOrDefault(x => x.Token == Token);
            _dbContext.Accounts.Remove(account);
            await _dbContext.SaveChangesAsync();
            return Ok(account.Name);
        }

        [HttpPost()]
        public async Task<Dictionary<string, string>?> Accounts()
        {
            var user = await _userManager.GetUserAsync(User);
            return await _dbContext.Accounts.Where(x => x.UserId == user.Id)?.ToDictionaryAsync(x => x.Name, x => x.Token);
        }
    }
}
