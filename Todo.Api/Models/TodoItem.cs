using System.Runtime.CompilerServices;

namespace Todo.Api.Models;

public enum PriorityLevel
{
    Low=0,
    Medium=1,
    High=2
}

public enum RecurrenceType
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4
}

public class TodoItem
{
    
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public bool IsPinned { get; set; }
    public string? Description { get; set; }
    public PriorityLevel Priority { get; set; } = PriorityLevel.Low;

    // Recurrence
    public bool IsRecurring { get; set; } = false;
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
    public int RecurrenceInterval { get; set; } = 1;
    public string? RecurrenceDaysOfWeek { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    // Sharing and Collaboration
    public bool IsShared { get; set; } = false;
    public string? SharedCode { get; set; }
    public int? CompletedByUserId { get; set; }
    public User? CompletedByUser { get; set; }

    public ICollection<TodoStep> Steps { get; set; } = new List<TodoStep>();
    public ICollection<TodoShare> Shares { get; set; } = new List<TodoShare>();
}
