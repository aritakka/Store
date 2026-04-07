using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        public string PasswordHash { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public int? RoleId { get; set; }
        public virtual Role? Role { get; set; }

        public virtual Customer? Customer { get; set; }
    }
}