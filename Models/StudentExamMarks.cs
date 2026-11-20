using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.Models
{
    public class StudentExamMarks
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required]
        [Display(Name = "Exam")]
        public int ExamId { get; set; }

        [Required]
        [Display(Name = "Obtained Marks")]
        public int ObtainedMarks { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [ForeignKey("ExamId")]
        public Exam? Exam { get; set; }

    }
}
