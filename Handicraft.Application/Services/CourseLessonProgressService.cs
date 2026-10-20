using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;
using System.Security.Claims;

namespace Handicraft.Application.Services;

public class CourseLessonProgressService
    : ICourseLessonProgressService
{
    private readonly ICourseLessonProgressRepository _progressRepository;
    private readonly ICourseLessonRepository _lessonRepository;
    private readonly ICourseEnrollmentRepository _enrollmentRepository;
    private readonly ICourseEpisodeRepository _episodeRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserCourseCategoryRepository _categoryRepository;

    public CourseLessonProgressService(
        ICourseLessonProgressRepository progressRepository,
        ICourseLessonRepository lessonRepository,
        ICourseEnrollmentRepository enrollmentRepository,
        ICourseEpisodeRepository episodeRepository,
        ICourseRepository courseRepository,
        IUserCourseCategoryRepository categoryRepository)
    {
        _progressRepository = progressRepository;
        _lessonRepository = lessonRepository;
        _enrollmentRepository = enrollmentRepository;
        _episodeRepository = episodeRepository;
        _courseRepository = courseRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<CourseLessonProgressDto?> GetEpisodeAsync(
        ClaimsPrincipal user,
        Guid lessonId,
        Guid episodeId)
    {
        var episode = await _episodeRepository.GetByIdAsync(episodeId);
        if (episode is null || episode.LessonId != lessonId)
            return null;

        if (!episode.IsFree && !await HasLessonAccessAsync(user, lessonId))
            throw new UnauthorizedAccessException("شما به این درس دسترسی ندارید.");

        var progress = await _progressRepository
            .GetByUserAndLessonAndEpisodeAsync(GetUserId(user), lessonId, episodeId);

        // Keep progress created before episode-specific tracking for single-video lessons.
        if (progress is null)
        {
            var episodes = await _episodeRepository.GetEpisodesByLessonIdAsync(lessonId);
            if (episodes.Count == 1)
                progress = await _progressRepository.GetByUserAndLessonAsync(GetUserId(user), lessonId);
        }

        return progress is null
            ? new CourseLessonProgressDto(lessonId, 0, false)
            : MapToDto(progress);
    }

    public async Task<CourseLessonProgressDto> UpdateEpisodeAsync(
        ClaimsPrincipal user,
        Guid lessonId,
        Guid episodeId,
        UpdateCourseLessonProgressDto dto)
    {
        if (dto.WatchedSeconds < 0)
            throw new ArgumentException("زمان مشاهده نمی‌تواند منفی باشد.");

        var episode = await _episodeRepository.GetByIdAsync(episodeId);
        if (episode is null || episode.LessonId != lessonId)
            throw new KeyNotFoundException("ویدیو پیدا نشد.");

        if (!episode.IsFree && !await HasLessonAccessAsync(user, lessonId))
            throw new UnauthorizedAccessException("شما به این درس دسترسی ندارید.");

        var userId = GetUserId(user);
        var progress = await _progressRepository
            .GetByUserAndLessonAndEpisodeAsync(userId, lessonId, episodeId);

        if (progress is null)
        {
            // Adopt legacy lesson progress only while this is the lesson's sole video.
            var episodes = await _episodeRepository.GetEpisodesByLessonIdAsync(lessonId);
            if (episodes.Count == 1)
                progress = await _progressRepository.GetByUserAndLessonAsync(userId, lessonId);
        }

        if (progress is null)
        {
            progress = new CourseLessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                EpisodeId = episodeId,
                WatchedSeconds = dto.WatchedSeconds,
                Completed = dto.Completed,
                CreatedAt = DateTime.UtcNow
            };
            await _progressRepository.AddAsync(progress);
        }
        else
        {
            progress.EpisodeId = episodeId;
            progress.WatchedSeconds = dto.WatchedSeconds;
            progress.Completed = dto.Completed;
            await _progressRepository.UpdateAsync(progress);
        }

        return MapToDto(progress);
    }

    public async Task<CourseLessonProgressDto?> GetAsync(
        ClaimsPrincipal user,
        Guid lessonId)
    {
        var userId = GetUserId(user);

        var lesson =
            await _lessonRepository.GetByIdAsync(lessonId);

        if (lesson is null)
            return null;

        var hasAccess =
            await HasLessonAccessAsync(
                user,
                lessonId);

        if (!hasAccess)
            throw new UnauthorizedAccessException(
                "شما به این درس دسترسی ندارید.");

        var progress =
            await _progressRepository
                .GetByUserAndLessonAsync(
                    userId,
                    lessonId);

        if (progress is null)
        {
            return new CourseLessonProgressDto(
                lessonId,
                0,
                false);
        }

        return MapToDto(progress);
    }

    public async Task<CourseLessonProgressDto> UpdateAsync(
        ClaimsPrincipal user,
        Guid lessonId,
        UpdateCourseLessonProgressDto dto)
    {
        var userId = GetUserId(user);

        if (dto.WatchedSeconds < 0)
        {
            throw new ArgumentException(
                "زمان مشاهده نمی‌تواند منفی باشد.");
        }

        var lesson =
            await _lessonRepository.GetByIdAsync(lessonId);

        if (lesson is null)
        {
            throw new KeyNotFoundException(
                "درس پیدا نشد.");
        }

        var hasAccess =
            await HasLessonAccessAsync(
                user,
                lessonId);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException(
                "شما به این درس دسترسی ندارید.");
        }

        var progress =
            await _progressRepository
                .GetByUserAndLessonAsync(
                    userId,
                    lessonId);

        if (progress is null)
        {
            progress = new CourseLessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                WatchedSeconds = dto.WatchedSeconds,
                Completed = dto.Completed,
                CreatedAt = DateTime.UtcNow
            };

            var created =
                await _progressRepository.AddAsync(
                    progress);

            return MapToDto(created);
        }

        progress.WatchedSeconds =
            Math.Max(
                progress.WatchedSeconds,
                dto.WatchedSeconds);

        if (dto.Completed)
        {
            progress.Completed = true;
        }

        await _progressRepository.UpdateAsync(
            progress);

        return MapToDto(progress);
    }

    private async Task<bool> HasLessonAccessAsync(
        ClaimsPrincipal user,
        Guid lessonId)
    {
        if (user.Identity?.IsAuthenticated != true)
            return false;

        if (user.IsInRole("Admin"))
            return true;

        var lesson =
            await _lessonRepository.GetByIdAsync(
                lessonId);

        if (lesson is null)
            return false;

        /*
         * اگر Lesson پیش‌نمایش رایگان باشد،
         * بدون Enrollment هم اجازه Progress داریم.
         */
        if (lesson.IsFreePreview)
            return true;

        var userId = GetUserId(user);

        /*
         * CourseId از:
         *
         * Lesson
         *   -> Section
         *      -> Course
         */
        var courseId =
            lesson.Section?.CourseId;

        if (courseId is null)
            return false;

        var hasEnrollment = await _enrollmentRepository
            .IsUserEnrolledAsync(userId, courseId.Value);
        if (hasEnrollment)
            return true;

        var course = await _courseRepository.GetByIdAsync(courseId.Value);
        return course is not null &&
            !string.IsNullOrWhiteSpace(course.Category) &&
            await _categoryRepository.HasAccessAsync(userId, course.Category);
    }

    private static Guid GetUserId(
        ClaimsPrincipal user)
    {
        var claim =
            user.FindFirst(
                ClaimTypes.NameIdentifier)
            ?? user.FindFirst("sub")
            ?? user.FindFirst("nameid");

        if (claim is null ||
            !Guid.TryParse(
                claim.Value,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "شناسه کاربر معتبر نیست.");
        }

        return userId;
    }

    private static CourseLessonProgressDto MapToDto(
        CourseLessonProgress progress)
    {
        return new CourseLessonProgressDto(
            progress.LessonId,
            progress.WatchedSeconds,
            progress.Completed);
    }
}
