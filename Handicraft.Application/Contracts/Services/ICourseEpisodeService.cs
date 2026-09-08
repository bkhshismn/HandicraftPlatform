using Handicraft.Application.Services.Dtos;
using System.Security.Claims;

namespace Handicraft.Application.Contracts.Services;

public interface ICourseEpisodeService
{
    Task<CourseEpisodeDto> CreateAndUploadAsync(
        Guid lessonId,
        CourseEpisodeUploadDto dto);


    Task<CourseEpisodeDto?> GetProcessingStatusAsync(
        Guid episodeId);


    Task<bool> UserHasAccessAsync(
        ClaimsPrincipal user,
        Guid lessonId,
        Guid episodeId);


    Task<string?> ResolveHlsFilePathAsync(
        Guid lessonId,
        Guid episodeId,
        string relativePath);


    Task<IReadOnlyList<CourseEpisodeDto>> GetEpisodesByLessonIdAsync(
        Guid lessonId);
    
        Task<bool> DeleteAsync(Guid lessonId, Guid episodeId);
}