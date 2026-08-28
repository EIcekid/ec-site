using EcSite.Api.Data;
using EcSite.Api.DTOs.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _db.Categories.AsNoTracking().ToListAsync();

        List<CategoryDto> BuildTree(int? parentId)
        {
            return categories.Where(c => c.ParentId == parentId)
                .Select(c => new CategoryDto(c.Id, c.Name, c.ParentId, BuildTree(c.Id)))
                .ToList();
        }

        return Ok(BuildTree(null));
    }
}
