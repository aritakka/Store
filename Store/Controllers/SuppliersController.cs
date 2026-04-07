using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;
using System.Linq;
using System.Security.Claims;

namespace Store.Controllers
{
    [Authorize(Roles = "Supplier")]
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Suppliers
        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Получаем все товары этого поставщика
            var products = _context.ProductSuppliers
                .Include(ps => ps.Product)
                    .ThenInclude(p => p.Category)
                .Where(ps => ps.SupplierId == userId)
                .Select(ps => ps.Product)
                .ToList();

            return View(products);
        }

        // GET: /Suppliers/Orders
        public IActionResult Orders()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Заказы на товары этого поставщика
            var orders = _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Customer)
                .Where(oi => oi.Product.ProductSuppliers.Any(ps => ps.SupplierId == userId))
                .Select(oi => oi.Order)
                .Distinct()
                .ToList();

            return View(orders);
        }
    }
}