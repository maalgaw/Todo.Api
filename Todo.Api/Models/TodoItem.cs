using System.Runtime.CompilerServices;

namespace Todo.Api.Models;

public enum PriorityLevel
{
    Low=0,
    Medium=1,
    High=2
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

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }
}
