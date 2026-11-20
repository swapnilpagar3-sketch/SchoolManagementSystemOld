using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.Models
{
    [Table("masUser")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string? Username { get; set; }

        [Column("Email")]
        public string? EmailAddress { get; set; }

        public string? Password { get; set; }
        public string? ResetToken { get;  set; }
        public DateTime? ResetTokenExpiry { get;  set; }
    }
}
