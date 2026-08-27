using Todo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Todo.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly TodoDbContext _context;
    public CategoriesController(TodoDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        var userId = GetCurrentUserId();
        var categories = await _context.Categories.Where(c => c.UserId == userId).ToListAsync();
        return Ok(categories);
    }
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory( Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest(new {message = "Tên thẻ không được để trống"});
        }
        category.UserId = GetCurrentUserId();
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return Ok(category);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, Category category)
    {
        if (id != category.Id)
        {
            return BadRequest(new {message = "ID thẻ không hợp lệ"});
        }
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest(new {message = "Tên thẻ không được để trống"});
        }
        
        var userId = GetCurrentUserId();
        var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (existingCategory == null)
        {
            return NotFound();
        }

        existingCategory.Name = category.Name;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var userId = GetCurrentUserId();
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (category == null)
        {
            return NotFound();
        }
        
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}
