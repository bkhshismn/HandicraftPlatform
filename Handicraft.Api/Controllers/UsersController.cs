using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Handicraft.Infrastructure.Persistence.Data;
using Handicraft.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly AppDbContext _db;

    public UsersController(IUserService userService, AppDbContext db)
    {
        _userService = userService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var users = await _db.Users.OrderBy(x => x.Role).ThenBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, Username = x.Username, x.Role, x.IsActive,
                Categories = x.CourseCategories.Select(c => c.Category).ToList() }).ToListAsync();
        var categories = await _db.CourseCategories.Select(x => x.Name).ToListAsync();
        categories = categories.Concat(CourseCategories.PapierMache).Concat(await _db.Courses.Select(x => x.Category).Distinct().ToListAsync()).Distinct().OrderBy(x => x).ToList();
        return Ok(new { users, categories });
    }

    [HttpPost("managed")]
    public async Task<IActionResult> CreateManaged([FromBody] CreateManagedUserRequest request)
    {
        var username = request.Username.Trim();
        if (string.IsNullOrWhiteSpace(username) || username.Length > 50 || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return BadRequest(new { message = "نام کاربری الزامی است و رمز باید حداقل ۸ نویسه باشد." });
        if (!await _userService.UsernameIsUniqueAsync(username)) return Conflict(new { message = "این نام کاربری قبلاً استفاده شده است." });
        var role = request.Role?.Trim();
        if (role is not ("User" or "Admin"))
            return BadRequest(new { message = "نقش کاربر معتبر نیست." });

        var user = new User { Id = Guid.NewGuid(), Username = username, Phone = username, Name = string.IsNullOrWhiteSpace(request.Name) ? username : request.Name.Trim(), PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), Role = role, IsActive = true, CreatedAt = DateTime.UtcNow };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Name, Username = user.Username, user.Role, user.IsActive, Categories = Array.Empty<string>() });
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> SetRole(Guid id, [FromBody] SetUserRoleRequest request)
    {
        var role = request.Role?.Trim();
        if (role is not ("User" or "Admin"))
            return BadRequest(new { message = "نقش کاربر معتبر نیست." });

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null) return NotFound();

        var currentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(currentId, out var currentUserId) && currentUserId == user.Id)
            return BadRequest(new { message = "نقش حساب خودتان را نمی‌توانید تغییر دهید." });

        if (user.Role == "Admin" && role != "Admin")
        {
            var adminCount = await _db.Users.CountAsync(x => x.Role == "Admin" && x.IsActive);
            if (adminCount <= 1)
                return BadRequest(new { message = "حداقل یک مدیر باید باقی بماند." });
        }

        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Role });
    }

    [HttpPut("{id:guid}/categories")]
    public async Task<IActionResult> SetCategories(Guid id, [FromBody] SetUserCategoriesRequest request)
    {
        var user = await _db.Users.Include(x => x.CourseCategories).Include(x => x.Enrollments).FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return NotFound();
        var categories = request.Categories.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToHashSet();
        _db.UserCourseCategories.RemoveRange(user.CourseCategories.ToList());
        foreach (var category in categories)
            _db.UserCourseCategories.Add(new UserCourseCategory { UserId = user.Id, Category = category });
        var courses = await _db.Courses.Where(x => categories.Contains(x.Category)).ToListAsync();
        var existing = user.Enrollments.ToDictionary(x => x.CourseId);
        foreach (var enrollment in user.Enrollments) enrollment.IsActive = courses.Any(c => c.Id == enrollment.CourseId);
        foreach (var course in courses)
        {
            if (existing.TryGetValue(course.Id, out var enrollment)) enrollment.IsActive = true;
            else _db.CourseEnrollments.Add(new CourseEnrollment { Id = Guid.NewGuid(), UserId = user.Id, CourseId = course.Id, PaidAmount = 0, EnrolledAt = DateTime.UtcNow, IsActive = true, CreatedAt = DateTime.UtcNow });
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:guid}/name")]
    public async Task<IActionResult> SetName(Guid id, [FromBody] SetUserNameRequest request)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100 || name.All(c => c == '?' || char.IsWhiteSpace(c)))
            return BadRequest(new { message = "نام نمایشی معتبر وارد کنید." });
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return NotFound();
        user.Name = name;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> Get(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet("by-phone/{phone}")]
    public async Task<ActionResult<UserDto>> GetByPhone(string phone)
    {
        var user = await _userService.GetByPhoneAsync(phone);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateDto dto)
    {
        if (dto == null) return BadRequest();

        var created = await _userService.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}

public sealed class CreateManagedUserRequest { public string Username { get; set; } = ""; public string Name { get; set; } = ""; public string Password { get; set; } = ""; public string Role { get; set; } = "User"; }
public sealed class SetUserRoleRequest { public string Role { get; set; } = "User"; }
public sealed class SetUserCategoriesRequest { public List<string> Categories { get; set; } = []; }
public sealed class SetUserNameRequest { public string Name { get; set; } = ""; }
