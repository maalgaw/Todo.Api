using System.ComponentModel.DataAnnotations;

namespace Todo.Api.Models.DTOs;

public class CreateTodoDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Description { get; set; }
    public PriorityLevel Priority { get; set; } = PriorityLevel.Low;
    public int? CategoryId { get; set; }
    public bool IsPinned { get; set; } = false;

    // Recurrence
    public bool IsRecurring { get; set; } = false;
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
    public int RecurrenceInterval { get; set; } = 1;
    public string? RecurrenceDaysOfWeek { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
}
