//using System.ComponentModel.DataAnnotations;

//namespace EntityFrameworkCore.Models
//{
//    public class Notification
//    {
//        public int Id { get; set; }

//        [Required]
//        [StringLength(200)]
//        public string Title { get; set; }

//        [Required]
//        [StringLength(500)]
//        public string Message { get; set; }

//        [Required]
//        [StringLength(50)]
//        public string NotificationType { get; set; }

//        // Indicates whether notification has been read
//        public bool IsRead { get; set; } = false;

//        // Date and time when notification was read
//        public DateTime? ReadDate { get; set; }

//        // Soft delete flag
//        public bool IsDeleted { get; set; } = false;

//        // Creation date
//        public DateTime CreatedDate { get; set; } = DateTime.Now;

//        // Optional URL to navigate when notification is clicked
//        [StringLength(300)]
//        public string? RedirectUrl { get; set; }

//        // Optional reference to related record
//        public int? ReferenceId { get; set; }
//    }
//}