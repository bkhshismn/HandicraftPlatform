using System.Collections.Generic;

namespace Handicraft.Application.Services.Dtos;

public record CourseDto(
    Guid Id,
    string Title,
    string Description,
    string CoverImageUrl,
    decimal Price,
    bool IsPublished,
    int DurationMinutes,
    IReadOnlyList<CourseEpisodeDto> Episodes
);
