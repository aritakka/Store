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

        // ✅ СТРАНИЦА ДЕТАЛЕЙ (ГЛАВНОЕ ЧТО НЕ ХВАТАЛО)
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

        // GET: /Products/DetailsJson/5
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

        // POST: /Products/EditJson/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJson(int id, [FromForm] ProductEditModel model)
        {
            if (id != model.Id)
                return BadRequest();

            var p = await _db.Products
                .Include(x => x.ProductSuppliers)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null)
                return NotFound();

            p.Name = model.Name;
            p.CategoryId = model.CategoryId;
            p.Price = model.Price;
            p.Quantity = model.Quantity;
            p.Description = model.Description;

            if (model.SupplierId.HasValue)
            {
                p.ProductSuppliers.Clear();

                p.ProductSuppliers.Add(new ProductSupplier
                {
                    ProductId = p.Id,
                    SupplierId = model.SupplierId.Value
                });
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }
    }

    // DTO для редактирования
    public class ProductEditModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public int? SupplierId { get; set; }

        public string? Description { get; set; }
    }
}