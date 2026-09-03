using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Models.DTOs;

namespace Todo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
// Controller xử lý thông tin cá nhân và quyền quản trị viên
public class UsersController : ControllerBase
{
    private readonly TodoDbContext _context;

    public UsersController(TodoDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "Không tìm thấy người dùng." });

        return Ok(new UserProfileDto
        {
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "Không tìm thấy người dùng." });

        // Chỉ cập nhật nếu có gửi giá trị
        if (dto.DisplayName != null)
        {
            user.DisplayName = dto.DisplayName;
        }

        if (dto.Email != null)
        {
            user.Email = dto.Email;
        }

        if (dto.AvatarUrl != null)
        {
            user.AvatarUrl = dto.AvatarUrl;
        }

        // Đổi mật khẩu nếu có gửi cả OldPassword và NewPassword
        if (!string.IsNullOrEmpty(dto.OldPassword) && !string.IsNullOrEmpty(dto.NewPassword))
        {
            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Mật khẩu cũ không chính xác." });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật hồ sơ thành công" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetAllUsers()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                DisplayName = u.DisplayName,
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("admin/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleDto dto)
    {
        if (dto.Role != "Admin" && dto.Role != "User")
            return BadRequest(new { message = "Role không hợp lệ." });

        if (id == GetCurrentUserId())
            return BadRequest(new { message = "Không thể tự thay đổi quyền của chính mình." });

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

        user.Role = dto.Role;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật quyền thành công." });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("admin/{id}")]
    public async Task<IActionResult> AdminUpdateUser(int id, [FromBody] AdminUpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

        if (dto.DisplayName != null)
            user.DisplayName = dto.DisplayName;

        if (dto.Email != null)
            user.Email = dto.Email;

        if (!string.IsNullOrEmpty(dto.NewPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        if (dto.Role != null)
        {
            if (dto.Role != "Admin" && dto.Role != "User")
                return BadRequest(new { message = "Role không hợp lệ." });

            if (id == GetCurrentUserId() && dto.Role != user.Role)
                return BadRequest(new { message = "Không thể tự thay đổi quyền của chính mình." });

            user.Role = dto.Role;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thông tin thành công." });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("admin/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

        // Xóa tất cả Todo của user
        var todos = await _context.TodoItems.Where(t => t.UserId == id).ToListAsync();
        _context.TodoItems.RemoveRange(todos);

        // Xóa tất cả category của user
        var categories = await _context.Categories.Where(c => c.UserId == id).ToListAsync();
        _context.Categories.RemoveRange(categories);

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã xóa người dùng thành công." });
    }
}
