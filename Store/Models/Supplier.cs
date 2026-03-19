using System.ComponentModel.DataAnnotations;
namespace Store.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public virtual ICollection<ProductSupplier> ProductSuppliers { get; set; }
    }
}