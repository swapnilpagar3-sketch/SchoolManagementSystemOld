using System.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.Models
{
    public class SignUpViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match.")]
        public string ConfirmPassword { get; set; }

        public string? ProfileImage { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
