using Handicraft.Domain.Entities;

namespace Handicraft.Application.Services.Dtos;

public record CourseEpisodeDto(
    Guid Id,
    string Title,
    string Description,
    string VideoUrl,
    int Order,
    int DurationSeconds,
    bool IsFree,
    VideoProcessingStatus Status
);