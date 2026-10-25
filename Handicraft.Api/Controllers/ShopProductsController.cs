using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/shop-products")]
public sealed class ShopProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List() => Ok(await db.ShopProducts.Where(x => x.IsPublished).OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpGet("manage")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Manage() => Ok(await db.ShopProducts.OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ShopProductRequest request)
    {
        var error = Validate(request);
        if (error is not null) return BadRequest(new { message = error });
        var product = Map(new ShopProduct(), request);
        await db.ShopProducts.AddAsync(product);
        await db.SaveChangesAsync();
        return Ok(product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string id, [FromBody] ShopProductRequest request)
    {
        var error = Validate(request);
        if (error is not null) return BadRequest(new { message = error });
        var product = await db.ShopProducts.FindAsync(id);
        if (product is null) return NotFound();
        Map(product, request);
        await db.SaveChangesAsync();
        return Ok(product);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var product = await db.ShopProducts.FindAsync(id);
        if (product is null) return NotFound();
        db.ShopProducts.Remove(product);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static string? Validate(ShopProductRequest x) =>
        string.IsNullOrWhiteSpace(x.Name) || x.Name.Trim().Length > 180 ? "نام محصول را وارد کنید (حداکثر ۱۸۰ نویسه)." :
        string.IsNullOrWhiteSpace(x.Category) || x.Category.Trim().Length > 100 ? "دسته‌بندی محصول را وارد کنید." :
        string.IsNullOrWhiteSpace(x.Description) || x.Description.Trim().Length > 2000 ? "توضیحات محصول را وارد کنید." :
        string.IsNullOrWhiteSpace(x.ImageUrl) || x.ImageUrl.Trim().Length > 600 ? "نشانی تصویر محصول را وارد کنید." : null;

    private static ShopProduct Map(ShopProduct p, ShopProductRequest x)
    {
        p.Name = x.Name.Trim(); p.Category = x.Category.Trim(); p.Description = x.Description.Trim();
        p.ImageUrl = x.ImageUrl.Trim(); p.Badge = (x.Badge ?? "دست‌ساز").Trim();
        p.Availability = (x.Availability ?? "هماهنگی با کارگاه").Trim();
        p.IsPublished = x.IsPublished; p.ImagePosition = string.IsNullOrWhiteSpace(x.ImagePosition) ? "center" : x.ImagePosition.Trim();
        return p;
    }
}

public sealed class ShopProductRequest
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string? Badge { get; set; }
    public string? Availability { get; set; }
    public string? ImagePosition { get; set; }
    public bool IsPublished { get; set; } = true;
}
