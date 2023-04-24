using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using RequiredAttribute = Microsoft.Build.Framework.RequiredAttribute;

namespace WebApiTraining1.Models
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(15, ErrorMessage = "The password must be no more than 15 characters.")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [StringLength(50, ErrorMessage = "The fullname must be no more than 50 characters.")]
        public string FullName { get; set; } = string.Empty;
    }
}
