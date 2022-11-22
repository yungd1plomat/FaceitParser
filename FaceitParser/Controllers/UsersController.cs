using FaceitParser.Data;
using FaceitParser.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FaceitParser.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;

        private readonly RoleManager<IdentityRole> roleManager;

        private readonly SignInManager<IdentityUser> signInManager;

        private readonly ApplicationDbContext context;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.context = dbContext;
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");
            return View(new LoginViewModel());
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return View(model);
        }

        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateUserViewModel model)
        {
            if (!roleManager.Roles.Any(x => x.Name == model.Role))
                return BadRequest(ModelState);
            var user = new IdentityUser(model.Username);
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var roleResult = await userManager.AddToRoleAsync(user, model.Role);
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RedirectToAction("Users", "Users", new { Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(string username)
        {
            var user = await userManager.FindByNameAsync(username);
            var logins = await userManager.GetLoginsAsync(user);
            var roles = await userManager.GetRolesAsync(user);
            if (user is null)
                return NotFound();
            if (logins is not null)
            {
                foreach (var login in logins)
                {
                    await userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
                }
            }
            if (roles is not null)
            {
                foreach (var role in roles)
                {
                    await userManager.RemoveFromRoleAsync(user, role);
                }
            }
            var blacklist = context.Blacklists.Where(x => x.UserId == user.Id).ToList();
            if (blacklist is not null && blacklist.Any())
            {
                context.Blacklists.RemoveRange(blacklist);
            }
            var accounts = context.Accounts.Where(x => x.UserId == user.Id).ToList();
            if (accounts is not null && accounts.Any())
            {
                context.Accounts.RemoveRange(accounts);
            }
            IdentityResult result = await userManager.DeleteAsync(user);
            await context.SaveChangesAsync();
            if (!result.Succeeded)
                return NotFound();
            return Ok(username);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromForm] EditUserViewmodel viewmodel)
        {
            var user = await userManager.FindByNameAsync(viewmodel.OldUsername);
            if (user is null)
                ModelState.AddModelError(string.Empty, "user not found");
            if (viewmodel.NewUsername is not null)
                user.UserName = viewmodel.NewUsername;
            if (viewmodel.Password is not null)
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, resetToken, viewmodel.Password);
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            if (viewmodel.Role is not null)
            {
                var oldRoles = await userManager.GetRolesAsync(user);
                var oldRole = oldRoles?.FirstOrDefault();
                if (oldRole != viewmodel.Role)
                {
                    if (oldRoles is not null && oldRoles.Any())
                    {
                        await userManager.RemoveFromRolesAsync(user, oldRoles);
                    }
                    var role = await roleManager.FindByNameAsync(viewmodel.Role);
                    if (role is null)
                        ModelState.AddModelError(string.Empty, "Role not found");
                    else
                        await userManager.AddToRoleAsync(user, viewmodel.Role);
                }
            }
            await userManager.UpdateAsync(user);
            return RedirectToAction("Users", "Users", new { Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
        }

        [HttpGet("dump/{username}")]
        public async Task<IActionResult> Dump(string username)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user is null)
                return NotFound();
            var blacklist = context.Blacklists.Where(x => x.UserId == user.Id).Select(x => x.ProfileId).ToList();
            var content = Encoding.UTF8.GetBytes(string.Join("\n", blacklist));
            var contentType = "text/plain";
            var fileName = $"{username}.txt";
            return File(content, contentType, fileName);
        }

        [HttpGet()]
        public async Task<IActionResult> Users(string? search = null, int? page = 0, IEnumerable<string>? Errors = null)
        {
            if (Errors is not null)
            {
                foreach (var error in Errors)
                    ModelState.AddModelError(string.Empty, error);
            }
            search = search?.ToLower();
            var users = search is null ? userManager.Users.ToList() : userManager.Users.Where(x => x.UserName.ToLower().Contains(search)).ToList();
            if (!users.Any())
                return View(new List<UserViewmodel>());
            List<UserViewmodel> vm = new List<UserViewmodel>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                UserViewmodel userViewmodel = new UserViewmodel()
                {
                    UserName = user.UserName,
                    Role = roles.Contains("admin") ? "admin" : "user",
                };
                vm.Add(userViewmodel);
            }
            var chunked = vm.Chunk(14).ToList();
            if (page >= chunked.Count())
                return RedirectToAction("Users", "Users");
            return View(chunked[page ?? 0].ToList());
        }
    }
}
