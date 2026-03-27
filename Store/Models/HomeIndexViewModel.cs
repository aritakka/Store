using System.Collections.Generic;

namespace Store.Models
{
    public class HomeIndexViewModel
    {
        public List<Product> Products { get; set; } = new();
        public Product? Featured { get; set; }
    }
}
