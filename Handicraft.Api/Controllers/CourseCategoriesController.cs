using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/course-categories")]
[Authorize(Roles = "Admin")]
public sealed class CourseCategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await db.CourseCategories.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseCategoryRequest request)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            return BadRequest(new { message = "نام دسته‌بندی باید بین ۱ تا ۱۰۰ نویسه باشد." });
        if (await db.CourseCategories.AnyAsync(x => x.Name.ToLower() == name.ToLower()))
            return Conflict(new { message = "این دسته‌بندی قبلاً ساخته شده است." });
        db.CourseCategories.Add(new CourseCategory { Name = name, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return Ok(name);
    }
}

public sealed class CreateCourseCategoryRequest { public string Name { get; set; } = string.Empty; }
