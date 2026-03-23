using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;
using System.Security.Claims;

public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var users = _context.Users.Include(u => u.Role).ToList();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create() => View(new User());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        if (!ModelState.IsValid) return View(user);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
        {
            ModelState.AddModelError(nameof(model.UserName), "Имя пользователя уже занято.");
            return View(model);
        }
        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email уже зарегистрирован.");
            return View(model);
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
        var user = new User
        {
            UserName = model.UserName,
            Email = model.Email,
            RoleId = role?.Id
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, role?.Name ?? "Customer")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        if (User?.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(AlreadyRegistered));

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _context.Users.Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.UserName == model.UserName || u.Email == model.UserName);

        if (user == null)
        {
            ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
            return View(model);
        }

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (verify == PasswordVerificationResult.Failed)
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
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AlreadyRegistered()
    {
        return View();
    }

    // Additional scaffolded actions (Edit, Details, Delete) can be kept as before or added here.
}
