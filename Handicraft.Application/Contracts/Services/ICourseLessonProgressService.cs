using Handicraft.Application.Services.Dtos;
using System.Security.Claims;

namespace Handicraft.Application.Contracts.Services;

public interface ICourseLessonProgressService
{
    Task<CourseLessonProgressDto?> GetAsync(
        ClaimsPrincipal user,
        Guid lessonId);

    Task<CourseLessonProgressDto> UpdateAsync(
        ClaimsPrincipal user,
        Guid lessonId,
        UpdateCourseLessonProgressDto dto);

    Task<CourseLessonProgressDto?> GetEpisodeAsync(
        ClaimsPrincipal user,
        Guid lessonId,
        Guid episodeId);

    Task<CourseLessonProgressDto> UpdateEpisodeAsync(
        ClaimsPrincipal user,
        Guid lessonId,
        Guid episodeId,
        UpdateCourseLessonProgressDto dto);
}
