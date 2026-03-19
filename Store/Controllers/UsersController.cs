using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Store.Data;
using Store.Models;
using System.Security.Claims;

public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<string> _passwordHasher;

    public UsersController(ApplicationDbContext context, IPasswordHasher<string> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // existing actions (Index, Create, etc.) remain...

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (_context.Users.Any(u => u.UserName == model.UserName))
        {
            ModelState.AddModelError(nameof(model.UserName), "Имя пользователя уже занято.");
            return View(model);
        }
        if (_context.Users.Any(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email уже зарегистрирован.");
            return View(model);
        }

        var user = new User
        {
            UserName = model.UserName,
            Email = model.Email,
            PasswordHash = _passwordHasher.HashPassword(null, model.Password),
            RoleId = _context.Roles.FirstOrDefault(r => r.Name == "Customer")?.Id
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        // Sign in
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "Customer")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity)).Wait();

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = _context.Users.SingleOrDefault(u => u.UserName == model.UserName || u.Email == model.UserName);
        if (user == null)
        {
            ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
            return View(model);
        }

        var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, model.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "Customer")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity)).Wait();

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
        return RedirectToAction("Index", "Home");
    }
}
