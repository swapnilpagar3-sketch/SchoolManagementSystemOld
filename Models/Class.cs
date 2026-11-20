using System.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; }
    }
}
