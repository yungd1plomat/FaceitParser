using FaceitParser.Data;
using FaceitParser.Models;
using FaceitParser.Services;
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

        private readonly SignInManager<ApplicationUser> _singInManager;

        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _singInManager = signInManager;
            _userManager = userManager;
        }

        [Route("/")]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}