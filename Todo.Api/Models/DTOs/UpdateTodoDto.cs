using System.ComponentModel.DataAnnotations;

namespace Todo.Api.Models.DTOs;

public class UpdateTodoDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Description { get; set; }
    public PriorityLevel Priority { get; set; }
    public int? CategoryId { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; } // Dùng cho chức năng Khôi phục (từ true -> false)

}
