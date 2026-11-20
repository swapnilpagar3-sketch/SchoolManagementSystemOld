using System.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.Models
{
    public class ResetPasswordViewModel
    {
        public string Token { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
