using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;

namespace Store.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // 🔹 Страница деталей
        // GET: /Products/Details/5
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

        // 🔹 JSON (если нужен)
        [HttpGet]
        public async Task<IActionResult> DetailsJson(int id)
        {
            var p = await _db.Products
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.ProductSuppliers)
                    .ThenInclude(ps => ps.Supplier)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null)
                return NotFound();

            var supplier = p.ProductSuppliers.FirstOrDefault()?.Supplier;

            return Json(new
            {
                p.Id,
                p.Name,
                p.CategoryId,
                Category = p.Category?.Name,
                p.Price,
                SupplierId = supplier?.Id,
                Supplier = supplier?.Name,
                p.Description,
                p.Quantity
            });
        }

        // 🔥 ГЛАВНОЕ — РАБОЧЕЕ СОХРАНЕНИЕ
        [HttpPost]
        [IgnoreAntiforgeryToken] // 💥 ОБЯЗАТЕЛЬНО
        public async Task<IActionResult> EditJson(int id, [FromForm] ProductEditModel model)
        {
            if (id != model.Id)
                return BadRequest();

            var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (p == null)
                return NotFound();

            // 🔥 Обновляем только нужные поля
            p.Price = model.Price;
            p.Quantity = model.Quantity;
            p.Description = model.Description;

            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }
    }

    // DTO
    public class ProductEditModel
    {
        public int Id { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public string? Description { get; set; }
    }
}