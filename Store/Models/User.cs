using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class User
    {
        [Key]
        public int IdUser { get; set; }
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
        [Range(0, 120)]
        public int Age { get; set; }
        public int IdRole { get; set; }
        [ForeignKey("IdRole")]
        public int Role { get; set; }
    }
}
