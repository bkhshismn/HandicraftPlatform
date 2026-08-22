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
}