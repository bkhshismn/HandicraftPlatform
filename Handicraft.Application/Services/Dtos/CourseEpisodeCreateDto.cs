namespace Handicraft.Application.Services.Dtos;

public record CourseEpisodeCreateDto(
    string Title,
    string Description,
    string VideoUrl,
    int Order,
    int DurationSeconds,
    bool IsFree
);
