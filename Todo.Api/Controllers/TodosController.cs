using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Models;
using Todo.Api.Models.DTOs;

namespace Todo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly TodoDbContext _context;

    public TodosController(TodoDbContext context)
    {
        _context = context;
    }

    //Lấy Id của người dùng qua token
    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    //GET
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos()
    {
        var userId = GetCurrentUserId();
        var todos = await _context.TodoItems
            .Include(t => t.Category)
            .Include(t => t.Steps)
            .Where(t => !t.IsDeleted && t.UserId == userId)
            .OrderByDescending(t => t.IsPinned)
            .ThenByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(todos);
    }

    //GET by id
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodo(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "ID công việc không hợp lệ (phải lớn hơn 0)." });
        }

        var userId = GetCurrentUserId();
        var todo = await _context.TodoItems
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null)
        {
            return NotFound(new { message = $"Không tìm thấy công việc." });
        }

        return Ok(todo);
    }

    //POST
    [HttpPost]
    public async Task<ActionResult<TodoItem>> CreateTodo(CreateTodoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Tên công việc không được để trống." });
        }

        if (dto.DueDate.HasValue && dto.DueDate.Value.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(new { message = "Hạn chót không được thiết lập trong quá khứ." });
        }

        var todo = new TodoItem
        {
            Title = dto.Title,
            IsCompleted = false,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            Description = dto.Description,
            Priority = dto.Priority,
            CategoryId = dto.CategoryId,
            IsPinned = dto.IsPinned,
            IsDeleted = false,
            UserId = GetCurrentUserId(),
            IsRecurring = dto.IsRecurring,
            RecurrenceType = dto.RecurrenceType,
            RecurrenceInterval = dto.RecurrenceInterval,
            RecurrenceDaysOfWeek = dto.RecurrenceDaysOfWeek,
            RecurrenceEndDate = dto.RecurrenceEndDate
        };

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
    }

    //PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodo(int id, UpdateTodoDto dto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "ID công việc không hợp lệ." });
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Tên công việc không được để trống." });
        }

        var userId = GetCurrentUserId();
        var todo = await _context.TodoItems
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null)
        {
            return NotFound(new { message = $"Không tìm thấy công việc." });
        }

        todo.Title = dto.Title;
        bool wasCompleted = todo.IsCompleted;
        todo.IsCompleted = dto.IsCompleted;
        todo.DueDate = dto.DueDate;
        todo.Description = dto.Description;
        todo.Priority = dto.Priority;
        todo.CategoryId = dto.CategoryId;
        todo.IsPinned = dto.IsPinned;
        todo.IsDeleted = dto.IsDeleted;
        todo.IsRecurring = dto.IsRecurring;
        todo.RecurrenceType = dto.RecurrenceType;
        todo.RecurrenceInterval = dto.RecurrenceInterval;
        todo.RecurrenceDaysOfWeek = dto.RecurrenceDaysOfWeek;
        todo.RecurrenceEndDate = dto.RecurrenceEndDate;

        // Xử lý khi đánh dấu hoàn thành công việc lặp lại
        if (!wasCompleted && dto.IsCompleted && todo.IsRecurring)
        {
            DateTime? nextDueDate = CalculateNextOccurrence(todo);
            
            // Nếu có ngày lặp tiếp theo và chưa vượt quá ngày kết thúc lặp lại
            if (nextDueDate.HasValue && (!todo.RecurrenceEndDate.HasValue || nextDueDate.Value <= todo.RecurrenceEndDate.Value))
            {
                var nextTodo = new TodoItem
                {
                    Title = todo.Title,
                    IsCompleted = false,
                    DueDate = nextDueDate,
                    CreatedAt = DateTime.UtcNow,
                    Description = todo.Description,
                    Priority = todo.Priority,
                    CategoryId = todo.CategoryId,
                    IsPinned = todo.IsPinned,
                    IsDeleted = false,
                    UserId = todo.UserId,
                    IsRecurring = todo.IsRecurring,
                    RecurrenceType = todo.RecurrenceType,
                    RecurrenceInterval = todo.RecurrenceInterval,
                    RecurrenceDaysOfWeek = todo.RecurrenceDaysOfWeek,
                    RecurrenceEndDate = todo.RecurrenceEndDate,
                    Steps = todo.Steps.Select(s => new TodoStep 
                    { 
                        Title = s.Title, 
                        IsCompleted = false 
                    }).ToList()
                };
                _context.TodoItems.Add(nextTodo);
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    //DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "ID công việc không hợp lệ." });
        }

        var userId = GetCurrentUserId();
        var todo = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null)
        {
            return NotFound(new { message = $"Không tìm thấy công việc." });
        }

        // Soft delete
        todo.IsDeleted = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    //GET Trash
    [HttpGet("trash")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTrashTodos()
    {
        var userId = GetCurrentUserId();
        var todos = await _context.TodoItems
            .Include(t => t.Category)
            .Where(t => t.IsDeleted && t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(todos);
    }

    //DELETE Trash (Hard delete)
    [HttpDelete("trash/{id}")]
    public async Task<IActionResult> HardDeleteTodo(int id)
    {
        var userId = GetCurrentUserId();
        var todo = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (todo == null)
        {
            return NotFound();
        }

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }



    private DateTime? CalculateNextOccurrence(TodoItem todo)
    {
        if (!todo.DueDate.HasValue) return null;
        DateTime baseDate = todo.DueDate.Value;
        int interval = todo.RecurrenceInterval > 0 ? todo.RecurrenceInterval : 1;

        switch (todo.RecurrenceType)
        {
            case RecurrenceType.Daily:
                return baseDate.AddDays(interval);
            case RecurrenceType.Weekly:
                // Nếu có chọn ngày cụ thể trong tuần (vd: 1,3,5 -> Thứ 2, Thứ 4, Thứ 6)
                if (!string.IsNullOrEmpty(todo.RecurrenceDaysOfWeek))
                {
                    var days = todo.RecurrenceDaysOfWeek.Split(',')
                        .Select(d => int.TryParse(d, out int parsed) ? parsed : -1)
                        .Where(d => d >= 0 && d <= 6)
                        .OrderBy(d => d)
                        .ToList();

                    if (days.Any())
                    {
                        int currentDayOfWeek = (int)baseDate.DayOfWeek;
                        // Chuyển Chủ nhật từ 0 thành 7 để dễ tính toán (nếu dùng quy ước 1=Thứ 2, 7=CN)
                        // Tuy nhiên C# DayOfWeek: 0=Sunday, 1=Monday... 6=Saturday.
                        // Giả sử giao diện gửi lên: 0=CN, 1=T2... 6=T7
                        
                        // Tìm ngày tiếp theo trong tuần này
                        int nextDay = days.FirstOrDefault(d => d > currentDayOfWeek);
                        if (nextDay > currentDayOfWeek)
                        {
                            return baseDate.AddDays(nextDay - currentDayOfWeek);
                        }
                        else
                        {
                            // Nhảy sang tuần tiếp theo (hoặc tuần sau nữa dựa vào interval)
                            int firstDayNextOccurrence = days.First();
                            int daysUntilNextWeek = 7 - currentDayOfWeek + firstDayNextOccurrence;
                            // Interval = 1 thì sang tuần sau, interval = 2 thì cách 1 tuần...
                            daysUntilNextWeek += (interval - 1) * 7;
                            return baseDate.AddDays(daysUntilNextWeek);
                        }
                    }
                }
                // Nếu không chọn ngày cụ thể, lặp lại đúng ngày đó tuần sau
                return baseDate.AddDays(7 * interval);
            case RecurrenceType.Monthly:
                return baseDate.AddMonths(interval);
            case RecurrenceType.Yearly:
                return baseDate.AddYears(interval);
            default:
                return null;
        }
    }
}
