using System.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.Models
{
    public class Exam
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Exam Name")]
        public string ExamName { get; set; }

        [Required]
        [Display(Name = "Exam Date")]
        [DataType(DataType.Date)]
        public DateTime ExamDate { get; set; }

        [Required]
        [Display(Name = "Total Marks")]
        [Range(1, 1000, ErrorMessage = "Total Marks must be greater than 0.")]
        public int TotalMarks { get; set; }

        [Required]
        [Display(Name = "Passing Marks")]
        [Range(1, 1000, ErrorMessage = "Passing Marks must be greater than 0.")]
        public int PassingMarks { get; set; }
    }
}
