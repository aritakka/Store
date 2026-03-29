using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;

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
            // Если уже аутентифицирован — не показываем форму регистрации
            if (User?.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("Authenticated user attempted to open Register page. Redirecting to Home.");
                return RedirectToAction("Index", "Home");
            }

            var model = new RegisterViewModel
            {
                IsAuthenticated = false,
                DisplayName = null
            };

            if (TempData["RegisterSuccess"] is string success)
                ViewBag.RegisterSuccess = success;

            return View("Register", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Если уже аутентифицирован — не позволяем повторно регистрироваться
            if (User?.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("Authenticated user attempted POST to Register. Redirecting to Home.");
                return RedirectToAction("Index", "Home");
            }

            if (model == null)
            {
                _logger.LogWarning("Register POST received null model.");
                return RedirectToAction("Register");
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

            TempData["RegisterSuccess"] = "Вы успешно зарегистрированы";
            return RedirectToAction("Register");
        }

        // Optional logout endpoints if still used elsewhere
        [HttpGet]
        public IActionResult AlreadyRegistered()
        {
            return View("AlreadyRegistered");
        }

        [HttpGet]
        public IActionResult SuccessLogin()
        {
            return View("SuccessLogin");
        }
    }
}
