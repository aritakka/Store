using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        // UI flags (устанавливаются контроллером, не формой)
        public bool IsAuthenticated { get; set; }
        public string? DisplayName { get; set; }
    }
}
