using System.ComponentModel.DataAnnotations;

namespace TaskFlow.API.DTOs
{
    public class TaskCreateDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        public string? Descripton { get; set; }
        public DateTime? DueDate { get; set; }
    }
}