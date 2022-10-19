using FaceitParser.Data;
using FaceitParser.Models;
using FaceitParser.Models.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FaceitParser.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;

        private readonly RoleManager<IdentityRole> roleManager;

        private readonly SignInManager<IdentityUser> signInManager;

        private readonly ApplicationDbContext context;

        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.context = dbContext;
        }

        [HttpGet("[controller]/login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");
            return View(new LoginViewModel());
        }

        [HttpPost("[controller]/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return View(model);
        }

        [HttpPost("[controller]/logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [Authorize(Roles = "admin")]
        [HttpPost("[controller]/create")]
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
            return RedirectToAction("Users", "Account", new { Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
        }

        [HttpPost("[controller]/delete")]
        [Authorize(Roles = "admin")]
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
            var blacklist = context.Blacklists.Where(x => x.UserId == user.Id);
            if (blacklist is not null && blacklist.Any())
            {
                context.Blacklists.RemoveRange(blacklist);
                await context.SaveChangesAsync();
            }
            IdentityResult result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return NotFound();
            return Ok(username);
        }

        [HttpPost("[controller]/edit")]
        [Authorize(Roles = "admin")]
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
            return RedirectToAction("Users", "Account", new { Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
        }

        [Authorize(Roles = "admin")]
        [HttpGet("users")]
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
                return RedirectToAction("Users", "Account");
            return View(chunked[page ?? 0].ToList());
        }


    }
}
