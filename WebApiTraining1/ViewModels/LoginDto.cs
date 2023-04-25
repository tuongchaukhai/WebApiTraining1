using System.ComponentModel.DataAnnotations;

namespace WebApiTraining1.ViewModels
{
    public class LoginDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(15, ErrorMessage = "The password must be no more than 15 characters.")]
        public string Password { get; set; } = string.Empty;
    }
}
