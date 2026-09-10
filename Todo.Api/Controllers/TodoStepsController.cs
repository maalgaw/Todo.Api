using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Todo.Api.Hubs;

namespace Todo.Api.Controllers;

public class CreateTodoStepDto
{
    public string Title { get; set; } = string.Empty;
}

public class UpdateTodoStepDto
{
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TodoStepsController : ControllerBase
{
    private readonly TodoDbContext _context;
    private readonly IHubContext<TodoHub> _hubContext;

    public TodoStepsController(TodoDbContext context, IHubContext<TodoHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    private async Task NotifySharedUsersAsync(TodoItem todoItem)
    {
        if (todoItem.IsShared || todoItem.Shares?.Any() == true)
        {
            var userIds = new List<string> { todoItem.UserId.ToString() };
            var sharedUserIds = await _context.TodoShares
                .Where(ts => ts.TodoItemId == todoItem.Id)
                .Select(ts => ts.UserId.ToString())
                .ToListAsync();
            userIds.AddRange(sharedUserIds);

            await _hubContext.Clients.Users(userIds.Distinct()).SendAsync("TodoUpdated");
        }
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    // POST /api/todosteps/{todoItemId}
    [HttpPost("{todoItemId}")]
    public async Task<ActionResult<TodoStep>> CreateTodoStep(int todoItemId, [FromBody] CreateTodoStepDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Tên bước không được để trống." });
        }

        var userId = GetCurrentUserId();
        var todoItem = await _context.TodoItems
            .Include(t => t.Shares)
            .FirstOrDefaultAsync(t => t.Id == todoItemId && (t.UserId == userId || t.Shares.Any(s => s.UserId == userId)));

        if (todoItem == null)
        {
            return NotFound(new { message = "Không tìm thấy công việc." });
        }

        var step = new TodoStep
        {
            Title = dto.Title,
            IsCompleted = false,
            TodoItemId = todoItemId
        };

        _context.TodoSteps.Add(step);
        await _context.SaveChangesAsync();
        await NotifySharedUsersAsync(todoItem);

        return Ok(step);
    }

    // PUT /api/todosteps/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodoStep(int id, [FromBody] UpdateTodoStepDto dto)
    {
        var userId = GetCurrentUserId();
        var step = await _context.TodoSteps
            .Include(s => s.TodoItem)
            .ThenInclude(t => t!.Shares)
            .Include(s => s.CompletedByUser)
            .FirstOrDefaultAsync(s => s.Id == id && s.TodoItem != null && (s.TodoItem.UserId == userId || s.TodoItem.Shares.Any(sh => sh.UserId == userId)));

        if (step == null)
        {
            return NotFound(new { message = "Không tìm thấy bước này." });
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Tên bước không được để trống." });
        }

        step.Title = dto.Title;
        bool wasCompleted = step.IsCompleted;
        step.IsCompleted = dto.IsCompleted;

        if (step.IsCompleted && !wasCompleted)
        {
            step.CompletedByUserId = userId;
        }
        else if (!step.IsCompleted)
        {
            step.CompletedByUserId = null;
        }

        await _context.SaveChangesAsync();
        await NotifySharedUsersAsync(step.TodoItem);

        return Ok(step);
    }

    // DELETE /api/todosteps/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoStep(int id)
    {
        var userId = GetCurrentUserId();
        var step = await _context.TodoSteps
            .Include(s => s.TodoItem)
            .ThenInclude(t => t!.Shares)
            .FirstOrDefaultAsync(s => s.Id == id && s.TodoItem != null && (s.TodoItem.UserId == userId || s.TodoItem.Shares.Any(sh => sh.UserId == userId)));

        if (step == null)
        {
            return NotFound(new { message = "Không tìm thấy bước này." });
        }

        _context.TodoSteps.Remove(step);
        await _context.SaveChangesAsync();
        await NotifySharedUsersAsync(step.TodoItem);

        return NoContent();
    }
}
