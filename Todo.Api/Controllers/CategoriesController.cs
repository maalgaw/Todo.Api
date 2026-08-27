using Todo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Todo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly TodoDbContext _context;
    public CategoriesController(TodoDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        return Ok(categories);
    }
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory( Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest(new {message = "Tên thẻ không được để trống"});
        }
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
        
        _context.Entry(category).State = EntityState.Modified;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Categories.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}

