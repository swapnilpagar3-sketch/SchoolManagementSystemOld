using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace EntityFrameworkCore.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Display(Name = "Student Name")]
        [Required]
        public string StudentName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Display(Name = "Date Of Birth")]
        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Mobile No")]
        [RegularExpression(@"^[0-9]{10}$",
        ErrorMessage = "Mobile Number must be exactly 10 digits.")]
        public string MobileNo { get; set; }

        [Required]
        public string Address { get; set; }

        [Display(Name = "Admission Date")]
        [Required]
        public DateTime AdmissionDate { get; set; }

        [Required]
        public int ClassId { get; set; }

        public ICollection<Attendance>? Attendances { get; set; }
    }
}
