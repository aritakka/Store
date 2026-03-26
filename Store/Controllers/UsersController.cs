using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;
using System.Security.Claims;

namespace Store.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, ILogger<UsersController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create() => View(new User());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string Password)
        {
            if (!ModelState.IsValid) return View(user);

            try
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, Password ?? string.Empty);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating user via Create() action for {UserName}", user.UserName);
                ModelState.AddModelError(string.Empty, "Ошибка при сохранении пользователя. Проверьте данные и повторите.");
                return View(user);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {
                IsAuthenticated = User?.Identity?.IsAuthenticated == true,
                DisplayName = User?.Identity?.Name
            };
            return View("Register", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model == null)
            {
                _logger.LogWarning("Register POST received null model.");
                return View("Register", new RegisterViewModel());
            }

            if (!ModelState.IsValid) return View("Register", model);

            if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError(nameof(model.UserName), "Имя пользователя уже занято.");
                return View("Register", model);
            }
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email уже зарегистрирован.");
                return View("Register", model);
            }

            // Ensure role exists (create if missing)
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            if (role == null)
            {
                role = new Role { Name = "Customer" };
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
            }

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                RoleId = role.Id
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating user {UserName}", model.UserName);
                ModelState.AddModelError(string.Empty, "Ошибка при сохранении пользователя. Возможно пользователь с таким именем или email уже существует.");
                return View("Register", model);
            }

            _logger.LogInformation("New user created: {UserName} (Id={Id})", user.UserName, user.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, role.Name)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            // Redirect back to Register GET so the view can show authenticated UI
            return RedirectToAction(nameof(Register));
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction(nameof(AlreadyRegistered));

            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View("Login", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (model == null)
                {
                    _logger.LogWarning("Login POST received null model.");
                    ModelState.AddModelError(string.Empty, "Некорректные данные.");
                    return View("Login", new LoginViewModel());
                }

                if (!ModelState.IsValid) return View("Login", model);

                var user = await _context.Users.Include(u => u.Role)
                    .SingleOrDefaultAsync(u => u.UserName == model.UserName || u.Email == model.UserName);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль.");
                    return View("Login", model);
                }

                var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                if (verify == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль.");
                    return View("Login", model);
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

                return RedirectToAction("Privacy", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Login for user {UserName}", model?.UserName);
                ModelState.AddModelError(string.Empty, "Внутренняя ошибка сервера. Подробности в логах.");
                return View("Login", model ?? new LoginViewModel());
            }
        }

        // POST logout (recommended)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET logout convenience endpoint (safe redirect) to allow link-based logout from views
        [HttpGet]
        public async Task<IActionResult> LogoutGet(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AlreadyRegistered()
        {
            return View("AlreadyRegistered");
        }
    }
}
