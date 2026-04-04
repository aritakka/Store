using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Store.Data;
using Store.Models;

using System.Diagnostics;
using System.Threading.Tasks;

namespace Store.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // GET: /
        public async Task<IActionResult> Index(string search, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            var productsQuery = _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Quantity > 0);

            // 🔍 Поиск
            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery.Where(p => EF.Functions.Like(p.Name, $"%{search}%"));
            }

            // 📂 Категория
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            // 💰 Цена от
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }

            // 💰 Цена до
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            var products = await productsQuery
                .OrderByDescending(p => p.Id)
                .Take(12)
                .ToListAsync();

            ViewData["Categories"] = categories;
            ViewData["SearchQuery"] = search;
            ViewData["CategoryId"] = categoryId;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}