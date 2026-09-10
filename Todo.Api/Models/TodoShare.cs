using System.Text.Json.Serialization;

namespace Todo.Api.Models;

public class TodoShare
{
    public int Id { get; set; }
    
    public int TodoItemId { get; set; }
    [JsonIgnore]
    public TodoItem? TodoItem { get; set; }
    
    public int UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    
    public DateTime SharedAt { get; set; } = DateTime.UtcNow;
}
