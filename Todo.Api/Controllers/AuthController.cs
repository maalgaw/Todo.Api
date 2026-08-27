using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Models;
using Todo.Api.Models.DTOs;
using Todo.Api.Services;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TodoDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(TodoDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        // 1. Kiểm tra xem Username đã tồn tại chưa
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == dto.Username.ToLower()))
        {
            return BadRequest(new { message = "Tên đăng nhập này đã có người sử dụng." });
        }

        // 2. Tạo User mới và Băm (Mã hóa) mật khẩu
        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 3. Cấp Token ngay sau khi đăng ký thành công
        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        // 1. Tìm user trong Database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());

        if (user == null) return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu." });

        // 2. So sánh mật khẩu giải mã
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu." });

        // 3. Cấp Token
        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl
        });
    }
}
