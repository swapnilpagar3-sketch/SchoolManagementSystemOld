using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.Models
{
    public class Fee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required]
        [Display(Name = "Fee Date")]
        [DataType(DataType.Date)]
        public DateTime FeeDate { get; set; }

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Mode")]
        public string PaymentMode { get; set; }

        public string? Remarks { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }
    }
}