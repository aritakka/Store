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

        // 🔹 Детали
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductSuppliers)
                .ThenInclude(ps => ps.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // 🔥 Редактирование
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EditJson(int id, [FromForm] ProductEditModel model)
        {
            if (!User.IsInRole("Admin"))
                return Unauthorized();

            var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (p == null)
                return NotFound();

            p.Price = model.Price;
            p.Quantity = model.Quantity;
            p.Description = model.Description;

            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }

        // 🔥🔥🔥 ЗАГРУЗКА КАРТИНКИ
        [HttpPost]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (!User.IsInRole("Admin"))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран");

            var path = Path.Combine(_env.WebRootPath, "images/products");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, $"{id}.jpg");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return RedirectToAction("Details", new { id });
        }
    }

    public class ProductEditModel
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
    }
}