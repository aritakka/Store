using Microsoft.AspNetCore.Mvc;
using Store.Data;
using Store.Models;
using System.Linq;

namespace Store.Controllers
{
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Roles
        public IActionResult Index()
        {
            var roles = _context.Roles.ToList();
            return View(roles);
        }

        // GET: Roles/Details/5
        public IActionResult Details(int id)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Id == id);
            if (role == null) return NotFound();
            return View(role);
        }

        // GET: Roles/Create
        public IActionResult Create() => View();

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Role role)
        {
            if (ModelState.IsValid)
            {
                _context.Roles.Add(role);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Roles/Edit/5
        public IActionResult Edit(int id)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Id == id);
            if (role == null) return NotFound();
            return View(role);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Role role)
        {
            if (id != role.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(role);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Roles/Delete/5
        public IActionResult Delete(int id)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Id == id);
            if (role == null) return NotFound();
            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var role = _context.Roles.Find(id);
            _context.Roles.Remove(role);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
