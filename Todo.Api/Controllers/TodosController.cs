using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Models;
using Todo.Api.Models.DTOs;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly TodoDbContext _context;

    public TodosController(TodoDbContext context)
    {
        _context = context;
    }

    //GET
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos()
    {
        var todos = await _context.TodoItems
            .Include(t => t.Category)
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.IsPinned)
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

        var todo = await _context.TodoItems.FindAsync(id);

        if (todo == null)
        {
            return NotFound(new { message = $"Không tìm thấy công việc với ID = {id}." });
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
            IsDeleted = false
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

        var todo = await _context.TodoItems.FindAsync(id);

        if (todo == null)
        {
            return NotFound(new { message = $"Không tìm thấy công việc với ID = {id} để sửa." });
        }

        todo.Title = dto.Title;
        todo.IsCompleted = dto.IsCompleted;
        todo.DueDate = dto.DueDate;
        todo.Description = dto.Description;
        todo.Priority = dto.Priority;
        todo.CategoryId = dto.CategoryId;
        todo.IsPinned = dto.IsPinned;
        todo.IsDeleted = dto.IsDeleted;

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

        var todo = await _context.TodoItems.FindAsync(id);

        if (todo == null)
        {
            return NotFound(new { message = $"Không tìm thấy công việc với ID = {id} để xóa." });
        }

        // Soft delete
        todo.IsDeleted = true;
        // _context.TodoItems.Remove(todo); // Old code
        await _context.SaveChangesAsync();

        return NoContent();
    }

    //GET Trash
    [HttpGet("trash")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTrashTodos()
    {
        var todos = await _context.TodoItems
            .Include(t => t.Category)
            .Where(t => t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(todos);
    }

    //DELETE Trash (Hard delete)
    [HttpDelete("trash/{id}")]
    public async Task<IActionResult> HardDeleteTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);

        if (todo == null)
        {
            return NotFound();
        }

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
