using Handicraft.Domain.Entities;
using System.Text.Json.Serialization;

namespace Handicraft.Application.Services.Dtos;

public record CourseEpisodeDto(
    Guid Id,
    string Title,
    string Description,
    string VideoUrl,
    int Order,
    int DurationSeconds,
    bool IsFree,

    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    VideoProcessingStatus Status
);