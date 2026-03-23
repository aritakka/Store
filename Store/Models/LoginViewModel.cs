using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; } // username or email

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
