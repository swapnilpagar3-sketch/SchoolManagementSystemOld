using System.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SubjectName { get; set; }
    }
}
