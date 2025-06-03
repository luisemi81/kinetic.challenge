using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinetic.Notification.Service.Models
{
    public class NotificationMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string EventType { get; set; } // "create", "update", "delete"

        [Required]
        public int ProductId { get; set; }

        [Required]
        public DateTime ReceivedAt { get; set; }

        [Required]
        public string Payload { get; set; }

        public string Status { get; set; } = "Received";
        public int ErrorCount { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
