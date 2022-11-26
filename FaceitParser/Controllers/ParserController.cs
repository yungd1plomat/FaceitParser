using FaceitParser.Abstractions;
using FaceitParser.Data;
using FaceitParser.Helpers;
using FaceitParser.Models;
using FaceitParser.Models.App;
using FaceitParser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FaceitParser.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class ParserController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IServiceResolver _serviceResolver;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _dbContext;

        public ParserController(IServiceResolver serviceResolver, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, ILogger<HomeController> logger)
        {
            _logger = logger;
            _serviceResolver = serviceResolver; 
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Parser(IEnumerable<string> Errors = null)
        {
            foreach (var error in Errors)
                ModelState.AddModelError(string.Empty, error);
            return View();
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> ParserView(string name)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            var service = _serviceResolver.Resolve(user.Id, name).FirstOrDefault();
            if (service == default)
                return RedirectToAction("Parser");
            return View("ParserView", service);
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateParserVm model)
        {
            if (!_dbContext.Accounts.Any(x => x.Token == model.Token))
                ModelState.AddModelError(string.Empty, "Аккаунт не найден");
            if (!ModelState.IsValid)
                return RedirectToAction("Parser", "Parser", new { Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)});
            CancellationTokenSource source = new CancellationTokenSource();
            FaceitApi faceitApi = new FaceitApi(model.Token, source.Token, model.Proxy, model.ProxyType);
            try
            {
                await faceitApi.GetSelf();
            } 
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Аккаунт, прокси или тип прокси неверны ({ex.Message})");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            var services = _serviceResolver.Resolve(user.Id);
            var location = Constants.Locations.FirstOrDefault(x => x.Name == model.Location);
            if (location is null)
                ModelState.AddModelError(string.Empty, $"Локация не найдена");
            if (services.Any(x => x.Location.Name == model.Location))
                ModelState.AddModelError(string.Empty, $"Парсер с такой локацией уже запущен");
            if (user is null)
                ModelState.AddModelError(string.Empty, "Пользователь не найден");
            if (services.Any(x => x.Name == model.Name))
                ModelState.AddModelError(string.Empty, "Парсер с таким названием уже существует");
            if (!ModelState.IsValid)
            {
                faceitApi.Dispose();
                source.Dispose();
                return RedirectToAction("Parser", "Parser", new { Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
            }
            await _serviceResolver.Create(user.Id, model.Name, faceitApi, location, model.Delay, model.MaxLvl, model.MaxMatches, model.MinPrice, model.AutoAdd, source);
            return Redirect($"~/parser/{model.Name}");
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(string name)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            _serviceResolver.Remove(user.Id, name);
            return Ok(name);
        }

        [HttpPost("{name}")]
        public async Task<IActionResult> GetData(string name)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            var service = _serviceResolver.Resolve(user.Id, name).FirstOrDefault();
            if (service == default)
                return RedirectToAction("Parser");
            GetParserViewmodel model = new GetParserViewmodel()
            {
                Account = service.AccountNick,
                Added = service.Added.value,
                Delay = service.Delay,
                Games = service.Games.value,
                Parsed = service.Parsed.value,
                Total = service.Total.value,
                Logs = service.Logs.DequeueAll(),
                SteamIds = service.SteamIds.Select(x => x.ToString()).ToList(),
            };
            return Ok(model);
        }
    }
}
