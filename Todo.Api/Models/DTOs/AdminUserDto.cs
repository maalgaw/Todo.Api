namespace Todo.Api.Models.DTOs;

public class AdminUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateRoleDto
{
    public string Role { get; set; } = string.Empty;
}
