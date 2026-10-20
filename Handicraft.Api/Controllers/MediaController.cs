using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/media")]
public sealed class MediaController : ControllerBase
{
    private const long MaxImageSize = 10 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> ImageTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
            [".gif"] = "image/gif"
        };

    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public MediaController(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    [HttpPost("images")]
    [Authorize(Roles = "Admin,Instructor")]
    [RequestSizeLimit(MaxImageSize + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxImageSize + 1024 * 1024)]
    public async Task<ActionResult<ImageUploadResponse>> UploadImage(
        [FromForm] ImageUploadRequest request,
        CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "تصویری انتخاب نشده است." });

        if (file.Length > MaxImageSize)
            return BadRequest(new { message = "حجم تصویر نباید بیشتر از ۱۰ مگابایت باشد." });

        var extension = Path.GetExtension(file.FileName);
        if (!ImageTypes.TryGetValue(extension, out var expectedContentType))
            return BadRequest(new { message = "فرمت تصویر پشتیبانی نمی‌شود." });

        if (!string.IsNullOrWhiteSpace(file.ContentType) &&
            !string.Equals(file.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(file.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "نوع فایل با فرمت تصویر همخوانی ندارد." });
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");

        var uploadDirectory = Path.Combine(webRoot, "uploads", "images");
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using (var output = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true))
        {
            await file.CopyToAsync(output, cancellationToken);
        }

        var publicBaseUrl = _configuration["Media:PublicBaseUrl"];
        if (string.IsNullOrWhiteSpace(publicBaseUrl))
            publicBaseUrl = $"{Request.Scheme}://{Request.Host}";

        var imageUrl =
            $"{publicBaseUrl.TrimEnd('/')}/uploads/images/{fileName}";

        return Ok(new ImageUploadResponse(imageUrl));
    }
}

public sealed record ImageUploadResponse(string Url);

public sealed class ImageUploadRequest
{
    public IFormFile File { get; set; } = null!;
}
