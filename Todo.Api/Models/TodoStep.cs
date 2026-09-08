using System.Text.Json.Serialization;

namespace Todo.Api.Models;

public class TodoStep
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    
    public int TodoItemId { get; set; }
    
    [JsonIgnore]
    public TodoItem? TodoItem { get; set; }
}
