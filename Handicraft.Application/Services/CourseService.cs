using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;
using Handicraft.Application.Contracts.Services;

namespace Handicraft.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;

    public CourseService(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<IEnumerable<CourseDto>> GetAllAsync()
    {
        var courses = await _courseRepository.GetAllWithEpisodesAsync();
        return courses.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<CourseDto>> GetPublishedAsync()
    {
        var courses = await _courseRepository.GetPublishedCoursesAsync();
        return courses.Select(MapToDto).ToList();
    }

    public async Task<CourseDto?> GetByIdAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdWithEpisodesAsync(id);
        if (course == null) return null;
        return MapToDto(course);
    }

    public async Task<CourseDto> CreateAsync(CourseCreateDto dto)
    {
        var course = new Course
        {
            Title = dto.Title,
            Description = dto.Description,
            CoverImageUrl = dto.CoverImageUrl,
            Price = dto.Price,
            IsPublished = dto.IsPublished,
            DurationMinutes = dto.DurationMinutes
        };

        if (dto.Episodes != null && dto.Episodes.Any())
        {
            foreach (var ep in dto.Episodes)
            {
                course.Episodes.Add(new CourseEpisode
                {
                    Id = Guid.NewGuid(),
                    Title = ep.Title,
                    Description = ep.Description,
                    VideoUrl = ep.VideoUrl,
                    Order = ep.Order,
                    DurationSeconds = ep.DurationSeconds,
                    IsFree = ep.IsFree,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var created = await _courseRepository.AddAsync(course);
        return MapToDto(created);
    }

    private static CourseDto MapToDto(Course c)
    {
        var episodes = c.Episodes?.OrderBy(e => e.Order).Select(e => new Dtos.CourseEpisodeDto(
            e.Id,
            e.Title,
            e.Description,
            e.VideoUrl,
            e.Order,
            e.DurationSeconds,
            e.IsFree
        )).ToList() ?? new List<CourseEpisodeDto>();

        return new CourseDto(
            c.Id,
            c.Title,
            c.Description,
            c.CoverImageUrl,
            c.Price,
            c.IsPublished,
            c.DurationMinutes,
            episodes
        );
    }
}
