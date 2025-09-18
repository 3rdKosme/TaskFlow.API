using System.ComponentModel.DataAnnotations;

namespace TaskFlow.API.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<TaskItem> Tasks { get; set; } = new();
    }
}