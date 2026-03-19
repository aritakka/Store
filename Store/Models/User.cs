using Store.Models;
using System.ComponentModel.DataAnnotations;
namespace Store.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }

        public int? RoleId { get; set; }  // Nullable RoleId
        public virtual Role? Role { get; set; }  // Nullable Role
    }
}