using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;

namespace Store.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AdminController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // ===== ДОБАВИТЬ ПОЛЬЗОВАТЕЛЯ =====

        [HttpGet]
        public IActionResult CreateUser()
        {
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string userName, string email, string password, int roleId)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Заполни все поля");
                ViewBag.Roles = _context.Roles.ToList();
                return View();
            }

            var user = new User
            {
                UserName = userName,
                Email = email,
                RoleId = roleId
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        // ===== ИЗМЕНИТЬ РОЛЬ =====

        [HttpGet]
        public IActionResult ChangeRole()
        {
            ViewBag.Users = _context.Users.Include(u => u.Role).ToList();
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int userId, int roleId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            user.RoleId = roleId;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}