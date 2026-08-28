using EcSite.Api.Data;
using EcSite.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcSite.Api.Controllers.Admin;

public record CreateCategoryRequest(string Name, int? ParentId);

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminCategoriesController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        var category = new Category { Name = request.Name, ParentId = request.ParentId };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return Ok(new { category.Id, category.Name, category.ParentId });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
