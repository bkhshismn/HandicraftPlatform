using AutoMapper;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;
using Handicraft.Application.Contracts.Services;

namespace Handicraft.Application.Services;

public class CourseEnrollmentService : ICourseEnrollmentService
{
    private readonly ICourseEnrollmentRepository _repo;
    private readonly IMapper _mapper;

    public CourseEnrollmentService(ICourseEnrollmentRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseEnrollmentDto>> GetEnrollmentsByUserAsync(Guid userId)
    {
        var items = await _repo.GetEnrollmentsByUserAsync(userId);
        return items.Select(i => _mapper.Map<CourseEnrollmentDto>(i)).ToList();
    }

    public async Task<IEnumerable<CourseEnrollmentDto>> GetEnrollmentsByCourseAsync(Guid courseId)
    {
        var items = await _repo.GetEnrollmentsByCourseAsync(courseId);
        return items.Select(i => _mapper.Map<CourseEnrollmentDto>(i)).ToList();
    }

    public async Task<CourseEnrollmentDto> CreateAsync(CourseEnrollmentCreateDto dto)
    {
        var e = _mapper.Map<CourseEnrollment>(dto);
        e.EnrolledAt = DateTime.UtcNow;
        e.Id = Guid.NewGuid();
        var created = await _repo.AddAsync(e);
        return _mapper.Map<CourseEnrollmentDto>(created);
    }
}
