namespace Todo.Api.Models.DTOs;

public class UpdateProfileDto
{
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    
    // Đổi mật khẩu
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
}
