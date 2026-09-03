using System.ComponentModel.DataAnnotations;

namespace Todo.Api.Models.DTOs;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;
}
