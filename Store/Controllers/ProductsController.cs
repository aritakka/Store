using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;

namespace Store.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // 🔹 Список всех товаров
        public async Task<IActionResult> Index()
        {
            var products = await _db.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // 🔹 Детали товара
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductSuppliers)
                .ThenInclude(ps => ps.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();
            return View(product);
        }

        // 🔹 Создание нового товара
        [HttpGet]
        public IActionResult Create()
        {
            if (!User.IsInRole("Admin")) return Unauthorized();
            ViewBag.Categories = _db.Categories.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? image)
        {
            if (!User.IsInRole("Admin")) return Unauthorized();
            if (!ModelState.IsValid) return View(product);

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            if (image != null)
            {
                var path = Path.Combine(_env.WebRootPath, "images/products");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var filePath = Path.Combine(path, $"{product.Id}.jpg");
                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);
            }

            return RedirectToAction(nameof(Index));
        }

        // 🔹 Редактирование товара
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!User.IsInRole("Admin")) return Unauthorized();
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = _db.Categories.ToList();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile? image)
        {
            if (!User.IsInRole("Admin")) return Unauthorized();
            if (!ModelState.IsValid) return View(product);

            _db.Products.Update(product);
            await _db.SaveChangesAsync();

            if (image != null)
            {
                var path = Path.Combine(_env.WebRootPath, "images/products");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var filePath = Path.Combine(path, $"{product.Id}.jpg");
                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);
            }

            return RedirectToAction(nameof(Index));
        }

        // 🔹 Удаление товара
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("Admin")) return Unauthorized();
            var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.IsInRole("Admin")) return Unauthorized();
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}