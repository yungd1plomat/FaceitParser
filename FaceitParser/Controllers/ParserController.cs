using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaceitParser.Controllers
{
    [Authorize]
    [Route("/")]
    public class ParserController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public ParserController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("parser")]
        public IActionResult Parser()
        {
            return View();
        }

    }
}
