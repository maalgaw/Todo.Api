using System.ComponentModel.DataAnnotations;

namespace Todo.Api.Models.DTOs;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Mã xác nhận không được để trống")]
    public string Code { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    [RegularExpression(@"^\S+$", ErrorMessage = "Mật khẩu không được chứa khoảng trắng")]
    public string NewPassword { get; set; } = string.Empty;
}
