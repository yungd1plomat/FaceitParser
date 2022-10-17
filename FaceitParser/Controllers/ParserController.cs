using FaceitParser.Abstractions;
using FaceitParser.Helpers;
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

        private readonly UserManager<IdentityUser> _userManager;

        public ParserController(IServiceResolver serviceResolver, UserManager<IdentityUser> userManager, ILogger<HomeController> logger)
        {
            _logger = logger;
            _serviceResolver = serviceResolver; 
            _userManager = userManager;
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
            var service = _serviceResolver.Resolve(user.Id, name).FirstOrDefault();
            if (service == default)
                return RedirectToAction("Parser");
            return View("ParserView", service);
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateParserVm model)
        {
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
                ModelState.AddModelError(string.Empty, $"Токен, прокси или тип прокси неверны ({ex.Message})");
            }
            var location = Constants.Locations.FirstOrDefault(x => x.Name == model.Location);
            if (location is null)
                ModelState.AddModelError(string.Empty, $"Локация не найдена");
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                ModelState.AddModelError(string.Empty, "Пользователь не найден");
            var service = _serviceResolver.Resolve(user.Id, model.Name);
            if (service.Any())
                ModelState.AddModelError(string.Empty, "Парсер с таким названием уже существует");
            if (!ModelState.IsValid)
            {
                faceitApi.Dispose();
                source.Dispose();
                return RedirectToAction("Parser", "Parser", new { Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
            }
            await _serviceResolver.Create(user.Id, model.Name, faceitApi, location, (int)model.Delay, model.MaxLvl, source);
            return Redirect($"~/parser/{model.Name}");
        }

        [HttpPost("{name}")]
        public async Task<IActionResult> GetData(string name)
        {
            var user = await _userManager.GetUserAsync(User);
            var service = _serviceResolver.Resolve(user.Id, name).FirstOrDefault();
            if (service == default)
                return RedirectToAction("Parser");
            GetParserViewmodel model = new GetParserViewmodel()
            {
                Account = service.faceitApi.SelfNick,
                Added = service.Added,
                Delay = service.Delay,
                Games = service.Games,
                Parsed = service.Parsed,
                Total = service.Total,
                Logs = service.Logs.DequeueAll(),
            };
            return Ok(model);
        }
    }
}
