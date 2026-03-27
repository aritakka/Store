using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;

namespace Store.Controllers
{
    // Поддерживаем оба пути: /Login/Login и /Users/Login
    [Route("[controller]/[action]")]
    [Route("Users/[action]")]
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<LoginController> _logger;

        public LoginController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, ILogger<LoginController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View("~/Views/Users/Login.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View("~/Views/Users/Login.cshtml", model);

            var user = await _context.Users.Include(u => u.Role)
                                           .FirstOrDefaultAsync(u => u.UserName == model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль.");
                return View("~/Views/Users/Login.cshtml", model);
            }

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, model.Password);
            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль.");
                return View("~/Views/Users/Login.cshtml", model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };
            if (user.Role != null) claims.Add(new Claim(ClaimTypes.Role, user.Role.Name ?? string.Empty));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = true });

            _logger.LogInformation("User {UserName} logged in (Id={Id})", user.UserName, user.Id);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("SuccessLogin", "Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Добавляем GET-эндпоинт, на который указывает шаблон в представлениях (/Users/LogoutGet),
        // но оставляем реализацию в LoginController — таким образом ссылки в представлениях продолжат работать,
        // и выход будет выполнен через LoginController.
        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}

