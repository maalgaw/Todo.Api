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
            Email = dto.Email,
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
            Email = user.Email,
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
            Email = user.Email,
            Role = user.Role,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto, [FromServices] IEmailService emailService)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            // Vẫn trả về OK để tránh lộ lọt email (bảo mật)
            return Ok(new { message = "Nếu email này tồn tại, mã xác nhận đã được gửi đi." });
        }

        // Tạo mã OTP ngẫu nhiên 6 số
        var otp = new Random().Next(100000, 999999).ToString();
        user.ResetPasswordCode = otp;
        user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(15);
        await _context.SaveChangesAsync();

        // Gửi email
        var subject = "Mã xác nhận khôi phục mật khẩu";
        var body = $@"
            <h3>Xin chào {user.DisplayName ?? user.Username},</h3>
            <p>Bạn vừa yêu cầu khôi phục mật khẩu cho tài khoản của mình trên TodoApp.</p>
            <p>Mã xác nhận của bạn là: <strong><span style='font-size:24px; color:blue;'>{otp}</span></strong></p>
            <p>Mã này có hiệu lực trong vòng 15 phút.</p>
            <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>
            <br/>
            <p>Trân trọng,<br/>TodoApp Team</p>
        ";

        try
        {
            await emailService.SendEmailAsync(user.Email, subject, body);
            return Ok(new { message = "Nếu email này tồn tại, mã xác nhận đã được gửi đi." });
        }
        catch (Exception)
        {
            // Log lỗi nếu cần thiết
            return StatusCode(500, new { message = "Có lỗi xảy ra khi gửi email xác nhận. Vui lòng thử lại sau." });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            return BadRequest(new { message = "Thông tin không hợp lệ." });
        }

        if (user.ResetPasswordCode != dto.Code || user.ResetPasswordExpiry < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Mã xác nhận không đúng hoặc đã hết hạn." });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.ResetPasswordCode = null;
        user.ResetPasswordExpiry = null;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại." });
    }
}
