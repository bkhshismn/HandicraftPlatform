using Handicraft.Application.Contracts.Persistence; // namespace صحیح
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;
using System.Security.Claims;

namespace Handicraft.Application.Services;

public class CourseEpisodeService : ICourseEpisodeService
{
    private readonly ICourseEpisodeRepository _episodeRepository;
    private readonly IVideoStorageService _videoStorage;

    public CourseEpisodeService(
        ICourseEpisodeRepository episodeRepository,
        IVideoStorageService videoStorage)
    {
        _episodeRepository = episodeRepository;
        _videoStorage = videoStorage;
    }

    public async Task<CourseEpisodeDto> CreateAndUploadAsync(
    Guid lessonId,
    CourseEpisodeUploadDto dto)
    {
        var episode = new CourseEpisode
        {
            Id = Guid.NewGuid(),

            LessonId = lessonId,

            Title = dto.Title,

            Description = dto.Description,

            VideoUrl = string.Empty,

            Order = dto.Order,

            DurationSeconds = 0,

            IsFree = dto.IsFree,

            Status = VideoProcessingStatus.Pending,

            CreatedAt = DateTime.UtcNow
        };

        await using var stream = dto.VideoFile.OpenReadStream();
        await _videoStorage.SaveRawVideoAsync(  lessonId,
                                                episode.Id,
                                                stream,
                                                dto.VideoFile.FileName);

        episode.RawVideoFileName = dto.VideoFile.FileName;



        var created = await _episodeRepository.AddAsync(episode);

        return MapToDto(created);
    }

    public async Task<CourseEpisodeDto?> GetProcessingStatusAsync(Guid episodeId)
    {
        var episode = await _episodeRepository.GetByIdAsync(episodeId);
        return episode is null ? null : MapToDto(episode);
    }

    public async Task<bool> UserHasAccessAsync(
     ClaimsPrincipal user,
     Guid lessonId,
     Guid episodeId)
    {
        var episode = await _episodeRepository.GetByIdAsync(episodeId);

        if (episode is null)
            return false;

        if (episode.IsFree)
            return true;

        // بعداً Enrollment را از Lesson -> Section -> Course چک می‌کنیم
        return false;
    }

    public async Task<string?> ResolveHlsFilePathAsync(
      Guid lessonId,
      Guid episodeId,
      string relativePath)
    {
        var episode =
            await _episodeRepository.GetByIdAsync(episodeId);

        if (episode is null ||
            episode.LessonId != lessonId ||
            episode.Status != VideoProcessingStatus.Ready)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        // جلوگیری از Path Traversal
        var normalizedPath =
            relativePath
                .Replace('\\', '/')
                .TrimStart('/');

        if (normalizedPath.Contains(".."))
            return null;

        var hlsFolder =
            _videoStorage.GetHlsOutputDirectory(
                lessonId,
                episodeId);

        var fullPath =
            Path.GetFullPath(
                Path.Combine(
                    hlsFolder,
                    normalizedPath));

        var fullHlsFolder =
            Path.GetFullPath(hlsFolder);

        if (!fullPath.StartsWith(
                fullHlsFolder,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return fullPath;
    }

    public async Task<IReadOnlyList<CourseEpisodeDto>> GetEpisodesByLessonIdAsync(  Guid lessonId)
    {
        var episodes = await _episodeRepository
            .GetEpisodesByLessonIdAsync(lessonId);

        return episodes
            .Select(MapToDto)
            .ToList();
    }

    private static CourseEpisodeDto MapToDto(CourseEpisode e) => new(
     e.Id,
     e.Title,
     e.Description,
     e.VideoUrl,
     e.Order,
     e.DurationSeconds,
     e.IsFree,
     e.Status
 );
}
