using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Models;

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

    public TodoStepsController(TodoDbContext context)
    {
        _context = context;
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
        var todoItem = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == todoItemId && t.UserId == userId);

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

        return Ok(step);
    }

    // PUT /api/todosteps/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodoStep(int id, [FromBody] UpdateTodoStepDto dto)
    {
        var userId = GetCurrentUserId();
        var step = await _context.TodoSteps
            .Include(s => s.TodoItem)
            .FirstOrDefaultAsync(s => s.Id == id && s.TodoItem != null && s.TodoItem.UserId == userId);

        if (step == null)
        {
            return NotFound(new { message = "Không tìm thấy bước này." });
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Tên bước không được để trống." });
        }

        step.Title = dto.Title;
        step.IsCompleted = dto.IsCompleted;

        await _context.SaveChangesAsync();

        return Ok(step);
    }

    // DELETE /api/todosteps/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoStep(int id)
    {
        var userId = GetCurrentUserId();
        var step = await _context.TodoSteps
            .Include(s => s.TodoItem)
            .FirstOrDefaultAsync(s => s.Id == id && s.TodoItem != null && s.TodoItem.UserId == userId);

        if (step == null)
        {
            return NotFound(new { message = "Không tìm thấy bước này." });
        }

        _context.TodoSteps.Remove(step);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
