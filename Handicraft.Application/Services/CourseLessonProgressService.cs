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

    public CourseLessonProgressService(
        ICourseLessonProgressRepository progressRepository,
        ICourseLessonRepository lessonRepository,
        ICourseEnrollmentRepository enrollmentRepository)
    {
        _progressRepository = progressRepository;
        _lessonRepository = lessonRepository;
        _enrollmentRepository = enrollmentRepository;
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

        return await _enrollmentRepository
            .IsUserEnrolledAsync(
                userId,
                courseId.Value);
    }

    private static Guid GetUserId(
        ClaimsPrincipal user)
    {
        var claim =
            user.FindFirst(
                ClaimTypes.NameIdentifier);

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