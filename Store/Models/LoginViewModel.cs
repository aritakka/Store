using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Имя пользователя или Email")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
