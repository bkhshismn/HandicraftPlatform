using Handicraft.Application.Services.Dtos;
using System.Security.Claims;

namespace Handicraft.Application.Contracts.Services;

public interface ICourseEpisodeService
{
    Task<CourseEpisodeDto> CreateAndUploadAsync(Guid courseId, CourseEpisodeUploadDto dto);
    Task<CourseEpisodeDto?> GetProcessingStatusAsync(Guid episodeId);
    Task<bool> UserHasAccessAsync(ClaimsPrincipal user, Guid courseId, Guid episodeId);
    Task<string?> ResolveHlsFilePathAsync(Guid courseId, Guid episodeId, string relativePath);
    Task<IReadOnlyList<CourseEpisodeDto>> GetEpisodesByCourseIdAsync(Guid courseId);
}
