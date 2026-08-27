namespace Todo.Api.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TodoItem> TodoItems { get; set; } = new();
    public int? UserId { get; set; }
    public User? User { get; set; }

}