using FaceitParser.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FaceitParser.Controllers
{
    [Authorize]
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly SignInManager<IdentityUser> _singInManager;

        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _singInManager = signInManager;
            _userManager = userManager;
        }

        [Route("/")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                await _singInManager.SignOutAsync();
            return View();
        }

        [Route("Error")]
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}