using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;

namespace Handicraft.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IVideoStorageService? _videoStorage;

    public CourseService(
        ICourseRepository courseRepository,
        IVideoStorageService? videoStorage = null)
    {
        _courseRepository = courseRepository;
        _videoStorage = videoStorage;
    }

    public async Task<CourseDto> CreateAsync(
        CourseCreateDto dto)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            CoverImageUrl = dto.CoverImageUrl,
            Price = dto.Price,
            IsPublished = dto.IsPublished,
            DurationMinutes = dto.DurationMinutes,
            CreatedAt = DateTime.UtcNow
        };

        var created =
            await _courseRepository.AddAsync(course);

        return MapToDto(created);
    }

    public async Task<IEnumerable<CourseDto>> GetAllAsync()
    {
        var courses =
            await _courseRepository.GetAllWithContentAsync();

        return courses
            .Select(MapToDto)
            .ToList();
    }

    public async Task<IEnumerable<CourseDto>> GetPublishedAsync()
    {
        var courses =
            await _courseRepository.GetPublishedCoursesAsync();

        return courses
            .Select(MapToDto)
            .ToList();
    }

    public async Task<CourseDto?> GetByIdAsync(Guid id)
    {
        var course =
            await _courseRepository.GetByIdWithContentAsync(id);

        if (course == null)
            return null;

        return MapToDto(course);
    }

    private static CourseDto MapToDto(Course c)
    {
        var sections =
            c.Sections?
                .OrderBy(x => x.OrderIndex)
                .Select(section =>
                    new CourseSectionDto
                    {
                        Id = section.Id,
                        CourseId = section.CourseId,
                        Title = section.Title,
                        OrderIndex = section.OrderIndex,

                        Lessons =
                            section.Lessons?
                                .OrderBy(x => x.OrderIndex)
                                .Select(lesson =>
                                {
                                    // یک Lesson می‌تواند چند Episode داشته باشد.
                                    // فعلاً اولین Episode را به عنوان ویدئوی اصلی
                                    // برای DTO فعلی انتخاب می‌کنیم.
                                    var episode =
                                     lesson.Episodes?
                                         .Where(x => x.Status == VideoProcessingStatus.Ready)
                                         .OrderByDescending(x => x.CreatedAt)
                                         .FirstOrDefault();

                                    return new CourseLessonDto
                                    {
                                        Id = lesson.Id,
                                        Title = lesson.Title,
                                        Description = lesson.Description,

                                        DurationSeconds =
                                            lesson.DurationSeconds,

                                        IsFreePreview =
                                            episode?.IsFree ?? lesson.IsFreePreview,

                                        OrderIndex =
                                            lesson.OrderIndex,

                                        EpisodeId =
                                            episode?.Id,

                                        VideoStatus =
                                            episode?.Status.ToString(),

                                        HasVideo =
                                            episode != null
                                    };
                                })
                                .ToList()
                            ?? new List<CourseLessonDto>()
                    })
                .ToList()
            ?? new List<CourseSectionDto>();

        return new CourseDto
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            CoverImageUrl = c.CoverImageUrl,
            Price = c.Price,
            IsFree = c.IsFree,
            IsPublished = c.IsPublished,
            Level = c.Level,
            Category = c.Category,
            DurationMinutes = c.DurationMinutes,
            Sections = sections
        };
    }
    public async Task<CourseDto?> SetPublishedAsync(
     Guid id,
     bool isPublished)
    {
        var course =
            await _courseRepository.GetByIdWithContentAsync(id);

        if (course is null)
            return null;

        course.IsPublished = isPublished;

        await _courseRepository.UpdateAsync(course);

        return MapToDto(course);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (_videoStorage is null)
            throw new InvalidOperationException(
                "Video storage is required to delete a course.");

        var course = await _courseRepository.GetByIdWithContentAsync(id);

        if (course is null)
            return false;

        var episodes = course.Sections
            .SelectMany(section => section.Lessons)
            .SelectMany(lesson => lesson.Episodes)
            .ToList();

        foreach (var episode in episodes)
        {
            await _videoStorage.DeleteEpisodeFilesAsync(
                episode.LessonId,
                episode.Id);
        }

        await _courseRepository.DeleteWithContentAsync(course);
        return true;
    }
}