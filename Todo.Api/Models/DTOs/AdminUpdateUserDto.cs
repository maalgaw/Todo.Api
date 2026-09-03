using System.ComponentModel.DataAnnotations;

namespace Todo.Api.Models.DTOs;

public class AdminUpdateUserDto
{
    public string? DisplayName { get; set; }
    
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
    public string? Email { get; set; }
    
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    [RegularExpression(@"^\S+$", ErrorMessage = "Mật khẩu không được chứa khoảng trắng")]
    public string? NewPassword { get; set; }
    
    public string? Role { get; set; }
}
