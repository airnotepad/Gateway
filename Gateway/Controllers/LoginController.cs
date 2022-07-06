using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gateway.Services;
using Gateway.Entity;

namespace Gateway.Controllers
{
    public class LoginController : Controller
    {
        private readonly Context _context;
        private readonly InviteService _invite;
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger, Context context, InviteService invite)
        {
            _context = context;
            _invite = invite;
            _logger = logger;
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInAsync(Models.View.LoginModel model, [FromQuery] string? ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);
                if (user != null)
                {
                    await Authenticate(user);

                    if (!string.IsNullOrEmpty(ReturnUrl))
                        return Redirect(ReturnUrl);
                    else
                        return RedirectToAction("Index", "Dashboard");
                }
                else
                    ModelState.AddModelError("", "Incorrect username or password");
            }
            return View(model);
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUpAsync(Models.View.RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                if (user == null)
                {
                    if (_invite.IsOk(model.Invite))
                    {
                        user = new User { Username = model.Username, Password = model.Password, Role = "User", Percent = 16 };

                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();

                        await Authenticate(user);

                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                        ModelState.AddModelError("", "Invite is not allowed");
                }
                else
                    ModelState.AddModelError("", "Username is not allowed");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Login");
        }

        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}
