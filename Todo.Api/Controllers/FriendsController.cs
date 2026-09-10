using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Models;

namespace Todo.Api.Controllers;

public class FriendRequestDto
{
    public string Email { get; set; } = string.Empty;
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FriendsController : ControllerBase
{
    private readonly TodoDbContext _context;

    public FriendsController(TodoDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    // Lấy danh sách bạn bè (Status = Accepted)
    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var userId = GetCurrentUserId();

        var friendships = await _context.Friendships
            .Include(f => f.Friend)
            .Include(f => f.User)
            .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendshipStatus.Accepted)
            .ToListAsync();

        var friends = friendships.Select(f =>
        {
            var isUserInitiator = f.UserId == userId;
            var friendUser = isUserInitiator ? f.Friend : f.User;
            
            return new
            {
                Id = friendUser!.Id,
                Username = friendUser.Username,
                Email = friendUser.Email,
                DisplayName = friendUser.DisplayName,
                AvatarUrl = friendUser.AvatarUrl
            };
        }).ToList();

        return Ok(friends);
    }

    // Lấy danh sách lời mời kết bạn ĐÃ NHẬN (Status = Pending, FriendId = current)
    [HttpGet("requests")]
    public async Task<IActionResult> GetFriendRequests()
    {
        var userId = GetCurrentUserId();

        var requests = await _context.Friendships
            .Include(f => f.User)
            .Where(f => f.FriendId == userId && f.Status == FriendshipStatus.Pending)
            .Select(f => new
            {
                FriendshipId = f.Id,
                User = new
                {
                    Id = f.User!.Id,
                    Username = f.User.Username,
                    Email = f.User.Email,
                    DisplayName = f.User.DisplayName,
                    AvatarUrl = f.User.AvatarUrl
                },
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    // Gửi lời mời kết bạn qua Email
    [HttpPost("add")]
    public async Task<IActionResult> AddFriend([FromBody] FriendRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Email không được để trống." });

        var userId = GetCurrentUserId();
        
        var friendUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (friendUser == null)
            return NotFound(new { message = "Không tìm thấy người dùng với Email này." });

        if (friendUser.Id == userId)
            return BadRequest(new { message = "Bạn không thể tự kết bạn với chính mình." });

        // Kiểm tra xem đã kết bạn hoặc đã gửi lời mời chưa
        var existingFriendship = await _context.Friendships
            .FirstOrDefaultAsync(f => (f.UserId == userId && f.FriendId == friendUser.Id) ||
                                      (f.UserId == friendUser.Id && f.FriendId == userId));

        if (existingFriendship != null)
        {
            if (existingFriendship.Status == FriendshipStatus.Accepted)
                return BadRequest(new { message = "Hai người đã là bạn bè." });
            else
                return BadRequest(new { message = "Lời mời kết bạn đang chờ xử lý." });
        }

        var friendship = new Friendship
        {
            UserId = userId,
            FriendId = friendUser.Id,
            Status = FriendshipStatus.Pending
        };

        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã gửi lời mời kết bạn." });
    }

    // Chấp nhận lời mời kết bạn
    [HttpPost("accept/{friendshipId}")]
    public async Task<IActionResult> AcceptFriendRequest(int friendshipId)
    {
        var userId = GetCurrentUserId();
        var friendship = await _context.Friendships.FirstOrDefaultAsync(f => f.Id == friendshipId && f.FriendId == userId);

        if (friendship == null)
            return NotFound(new { message = "Không tìm thấy lời mời kết bạn." });

        if (friendship.Status == FriendshipStatus.Accepted)
            return BadRequest(new { message = "Lời mời đã được chấp nhận trước đó." });

        friendship.Status = FriendshipStatus.Accepted;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã chấp nhận lời mời kết bạn." });
    }

    // Từ chối/Xóa bạn bè
    [HttpDelete("{friendshipId}")]
    public async Task<IActionResult> RemoveFriend(int friendshipId)
    {
        var userId = GetCurrentUserId();
        var friendship = await _context.Friendships.FirstOrDefaultAsync(f => 
            f.Id == friendshipId && (f.UserId == userId || f.FriendId == userId));

        if (friendship == null)
            return NotFound(new { message = "Không tìm thấy mối quan hệ này." });

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã xóa thành công." });
    }
}
