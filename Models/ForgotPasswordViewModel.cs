using System.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}
