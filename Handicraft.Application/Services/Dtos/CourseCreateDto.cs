using System.Collections.Generic;

namespace Handicraft.Application.Services.Dtos;

public record CourseCreateDto(
    string Title,
    string Description,
    string CoverImageUrl,
    decimal Price,
    bool IsPublished,
    int DurationMinutes,
    IReadOnlyList<CourseEpisodeCreateDto>? Episodes = null,
    string Category = "عمومی"
);
