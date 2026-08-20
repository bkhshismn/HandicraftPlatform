using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;
using System.Security.Claims;

namespace Handicraft.Application.Services;

public class CourseEpisodeService : ICourseEpisodeService
{
    private readonly ICourseEpisodeRepository _episodeRepository;
    private readonly ICourseEnrollmentRepository _enrollmentRepository;
    private readonly IVideoStorageService _videoStorage;

    public CourseEpisodeService(
        ICourseEpisodeRepository episodeRepository,
        ICourseEnrollmentRepository enrollmentRepository,
        IVideoStorageService videoStorage)
    {
        _episodeRepository = episodeRepository;
        _enrollmentRepository = enrollmentRepository;
        _videoStorage = videoStorage;
    }

    // ============================================================
    // Create + Upload Episode
    // ============================================================

    public async Task<CourseEpisodeDto> CreateAndUploadAsync(
        Guid lessonId,
        CourseEpisodeUploadDto dto)
    {
        if (dto.VideoFile is null ||
            dto.VideoFile.Length == 0)
        {
            throw new InvalidOperationException(
                "فایل ویدئو انتخاب نشده است.");
        }

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

            CreatedAt = DateTime.UtcNow,

            RawVideoFileName = dto.VideoFile.FileName
        };

        // --------------------------------------------------------
        // Save raw video
        // --------------------------------------------------------

        await using var stream =
            dto.VideoFile.OpenReadStream();

        await _videoStorage.SaveRawVideoAsync(
            lessonId,
            episode.Id,
            stream,
            dto.VideoFile.FileName);

        // --------------------------------------------------------
        // Save episode in database
        // --------------------------------------------------------

        var created =
            await _episodeRepository.AddAsync(episode);

        Console.WriteLine(
            $"[EPISODE CREATED] Id={created.Id}");

        Console.WriteLine(
            $"[EPISODE CREATED] LessonId={created.LessonId}");

        Console.WriteLine(
            $"[EPISODE CREATED] Title={created.Title}");

        Console.WriteLine(
            $"[EPISODE CREATED] Description={created.Description}");

        Console.WriteLine(
            $"[EPISODE CREATED] Order={created.Order}");

        Console.WriteLine(
            $"[EPISODE CREATED] IsFree={created.IsFree}");

        Console.WriteLine(
            $"[EPISODE CREATED] Status={created.Status}");

        Console.WriteLine(
            $"[EPISODE CREATED] RawVideo={created.RawVideoFileName}");

        return MapToDto(created);
    }


    // ============================================================
    // Processing Status
    // ============================================================

    public async Task<CourseEpisodeDto?> GetProcessingStatusAsync(
        Guid episodeId)
    {
        var episode =
            await _episodeRepository.GetByIdAsync(
                episodeId);

        if (episode is null)
            return null;

        return MapToDto(episode);
    }


    // ============================================================
    // Video Access Control
    //
    // Rules:
    //
    // 1. Episode must exist
    // 2. LessonId must match
    // 3. Video must be Ready
    // 4. Free episode -> everyone can watch
    // 5. Admin -> access granted
    // 6. Paid episode -> authentication required
    // 7. Paid episode -> active enrollment required
    // ============================================================

    public async Task<bool> UserHasAccessAsync(
        ClaimsPrincipal user,
        Guid lessonId,
        Guid episodeId)
    {
        Console.WriteLine();
        Console.WriteLine(
            "========== VIDEO ACCESS CHECK ==========");

        Console.WriteLine(
            $"[VIDEO ACCESS] LessonId={lessonId}");

        Console.WriteLine(
            $"[VIDEO ACCESS] EpisodeId={episodeId}");

        Console.WriteLine(
            $"[VIDEO ACCESS] User={user?.Identity?.Name}");

        Console.WriteLine(
            $"[VIDEO ACCESS] Authenticated=" +
            $"{user?.Identity?.IsAuthenticated}");

        Console.WriteLine(
            $"[VIDEO ACCESS] Admin=" +
            $"{user?.IsInRole("Admin")}");

        // ========================================================
        // 1. Get Episode + Lesson + Section + Course
        // ========================================================

        var episode =
            await _episodeRepository.GetEpisodeWithCourseAsync(
                episodeId);

        if (episode is null)
        {
            Console.WriteLine(
                "[VIDEO ACCESS] Episode not found.");

            return false;
        }

        // ========================================================
        // 2. Verify LessonId
        // ========================================================

        if (episode.LessonId != lessonId)
        {
            Console.WriteLine(
                $"[VIDEO ACCESS] Lesson mismatch. " +
                $"Expected={lessonId}, " +
                $"Actual={episode.LessonId}");

            return false;
        }

        // ========================================================
        // 3. Video must be Ready
        // ========================================================

        if (episode.Status != VideoProcessingStatus.Ready)
        {
            Console.WriteLine(
                $"[VIDEO ACCESS] Video is not ready. " +
                $"Status={episode.Status}");

            return false;
        }

        // ========================================================
        // 4. FREE EPISODE
        //
        // IMPORTANT:
        // This MUST happen before authentication check.
        //
        // Anonymous users can watch free previews.
        // ========================================================

        if (episode.IsFree)
        {
            Console.WriteLine(
                "[VIDEO ACCESS] Episode is FREE.");

            Console.WriteLine(
                "[VIDEO ACCESS] Access granted.");

            return true;
        }

        // ========================================================
        // 5. Paid episode requires authentication
        // ========================================================

        if (user?.Identity?.IsAuthenticated != true)
        {
            Console.WriteLine(
                "[VIDEO ACCESS] Paid episode.");

            Console.WriteLine(
                "[VIDEO ACCESS] User is not authenticated.");

            Console.WriteLine(
                "[VIDEO ACCESS] Access denied.");

            return false;
        }

        // ========================================================
        // 6. Admin access
        // ========================================================

        if (user.IsInRole("Admin"))
        {
            Console.WriteLine(
                "[VIDEO ACCESS] Admin access granted.");

            return true;
        }

        // ========================================================
        // 7. Get UserId from JWT
        // ========================================================

        var userIdClaim =
            user.FindFirst(
                ClaimTypes.NameIdentifier);

        if (userIdClaim is null)
        {
            Console.WriteLine(
                "[VIDEO ACCESS] NameIdentifier claim not found.");

            return false;
        }

        if (!Guid.TryParse(
                userIdClaim.Value,
                out var userId))
        {
            Console.WriteLine(
                $"[VIDEO ACCESS] Invalid UserId claim: " +
                $"{userIdClaim.Value}");

            return false;
        }

        Console.WriteLine(
            $"[VIDEO ACCESS] UserId={userId}");

        // ========================================================
        // 8. Resolve Course
        //
        // Episode
        //    ↓
        // Lesson
        //    ↓
        // Section
        //    ↓
        // Course
        // ========================================================

        var course =
            episode.Lesson?
                .Section?
                .Course;

        if (course is null)
        {
            Console.WriteLine(
                "[VIDEO ACCESS] Course could not be resolved.");

            return false;
        }

        Console.WriteLine(
            $"[VIDEO ACCESS] CourseId={course.Id}");

        // ========================================================
        // 9. Check enrollment
        // ========================================================

        var isEnrolled =
            await _enrollmentRepository
                .IsUserEnrolledAsync(
                    userId,
                    course.Id);

        Console.WriteLine(
            $"[VIDEO ACCESS] IsEnrolled={isEnrolled}");

        if (!isEnrolled)
        {
            Console.WriteLine(
                "[VIDEO ACCESS] User is NOT enrolled.");

            Console.WriteLine(
                "[VIDEO ACCESS] Access denied.");

            return false;
        }

        // ========================================================
        // 10. Access granted
        // ========================================================

        Console.WriteLine(
            "[VIDEO ACCESS] User is enrolled.");

        Console.WriteLine(
            "[VIDEO ACCESS] Access granted.");

        return true;
    }


    // ============================================================
    // Resolve HLS File Path
    // ============================================================

    public async Task<string?> ResolveHlsFilePathAsync(
        Guid lessonId,
        Guid episodeId,
        string relativePath)
    {
        // --------------------------------------------------------
        // Get episode
        // --------------------------------------------------------

        var episode =
            await _episodeRepository.GetByIdAsync(
                episodeId);

        if (episode is null)
        {
            Console.WriteLine(
                "[HLS PATH] Episode not found.");

            return null;
        }

        // --------------------------------------------------------
        // Verify lesson
        // --------------------------------------------------------

        if (episode.LessonId != lessonId)
        {
            Console.WriteLine(
                "[HLS PATH] Lesson mismatch.");

            return null;
        }

        // --------------------------------------------------------
        // Video must be ready
        // --------------------------------------------------------

        if (episode.Status != VideoProcessingStatus.Ready)
        {
            Console.WriteLine(
                $"[HLS PATH] Episode is not ready. " +
                $"Status={episode.Status}");

            return null;
        }

        // --------------------------------------------------------
        // Validate relative path
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            Console.WriteLine(
                "[HLS PATH] Relative path is empty.");

            return null;
        }

        var normalizedPath =
            relativePath
                .Replace('\\', '/')
                .TrimStart('/');

        // --------------------------------------------------------
        // Prevent path traversal
        // --------------------------------------------------------

        if (normalizedPath.Contains(".."))
        {
            Console.WriteLine(
                "[HLS PATH] Path traversal attempt blocked.");

            return null;
        }

        // --------------------------------------------------------
        // Get HLS folder
        // --------------------------------------------------------

        var hlsFolder =
            _videoStorage.GetHlsOutputDirectory(
                lessonId,
                episodeId);

        if (string.IsNullOrWhiteSpace(hlsFolder))
        {
            Console.WriteLine(
                "[HLS PATH] HLS folder is empty.");

            return null;
        }

        // --------------------------------------------------------
        // Resolve full paths
        // --------------------------------------------------------

        var fullHlsFolder =
            Path.GetFullPath(
                hlsFolder);

        var fullPath =
            Path.GetFullPath(
                Path.Combine(
                    fullHlsFolder,
                    normalizedPath));

        // --------------------------------------------------------
        // Make sure file remains inside HLS folder
        //
        // Add directory separator to avoid cases like:
        //
        // C:\Video\episode
        // C:\Video\episode-other
        // --------------------------------------------------------

        var hlsRoot =
            fullHlsFolder.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                hlsRoot,
                StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(
                "[HLS PATH] Resolved path is outside HLS folder.");

            return null;
        }

        Console.WriteLine(
            $"[HLS PATH] Resolved: {fullPath}");

        return fullPath;
    }


    // ============================================================
    // Get Episodes By Lesson
    // ============================================================

    public async Task<IReadOnlyList<CourseEpisodeDto>>
        GetEpisodesByLessonIdAsync(
            Guid lessonId)
    {
        var episodes =
            await _episodeRepository
                .GetEpisodesByLessonIdAsync(
                    lessonId);

        return episodes
            .Select(MapToDto)
            .ToList();
    }


    // ============================================================
    // Entity -> DTO
    // ============================================================

    private static CourseEpisodeDto MapToDto(
        CourseEpisode e)
    {
        return new CourseEpisodeDto(
            e.Id,
            e.Title,
            e.Description,
            e.VideoUrl,
            e.Order,
            e.DurationSeconds,
            e.IsFree,
            e.Status);
    }
}