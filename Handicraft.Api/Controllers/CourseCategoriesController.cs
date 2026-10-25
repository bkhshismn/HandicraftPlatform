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

    [HttpPut]
    public async Task<IActionResult> Rename([FromBody] RenameCourseCategoryRequest request)
    {
        var oldName = request.OldName?.Trim();
        var newName = request.NewName?.Trim();
        if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName) || newName.Length > 100)
            return BadRequest(new { message = "نام فعلی و نام جدید دسته را وارد کنید." });
        var category = await db.CourseCategories.FirstOrDefaultAsync(x => x.Name == oldName);
        if (category is null) return NotFound();
        if (await db.CourseCategories.AnyAsync(x => x.Name.ToLower() == newName.ToLower() && x.Name != oldName))
            return Conflict(new { message = "دسته‌ای با این عنوان از قبل وجود دارد." });
        category.Name = newName;
        await db.SaveChangesAsync();
        return Ok(newName);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string name)
    {
        var category = await db.CourseCategories.FirstOrDefaultAsync(x => x.Name == name);
        if (category is null) return NotFound();
        var assigned = await db.UserCourseCategories.AnyAsync(x => x.Category == name);
        if (assigned) return Conflict(new { message = "این دسته به دسترسی کاربران وصل است؛ ابتدا دسترسی‌های مرتبط را حذف کنید." });
        db.CourseCategories.Remove(category);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public sealed class CreateCourseCategoryRequest { public string Name { get; set; } = string.Empty; }
public sealed class RenameCourseCategoryRequest { public string OldName { get; set; } = string.Empty; public string NewName { get; set; } = string.Empty; }
