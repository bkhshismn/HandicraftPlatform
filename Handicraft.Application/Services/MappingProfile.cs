using AutoMapper;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;


namespace Handicraft.Application.Services;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Course
        CreateMap<Course, CourseDto>().MaxDepth(64);
        CreateMap<CourseSection, CourseSectionDto>().MaxDepth(64);
        CreateMap<CourseLesson, CourseLessonDto>().MaxDepth(64);
        CreateMap<CourseEpisode, CourseEpisodeDto>().MaxDepth(64);
        CreateMap<CourseCreateDto, Course>();
        CreateMap<CourseEpisodeCreateDto, CourseEpisode>();

        // User
        CreateMap<User, UserDto>();
        CreateMap<UserCreateDto, User>();

        // Order
        CreateMap<Order, OrderDto>();
        CreateMap<OrderCreateDto, Order>();

        // Portfolio
        CreateMap<PortfolioImage, PortfolioImageDto>();
        CreateMap<PortfolioItem, PortfolioItemDto>();
        CreateMap<PortfolioCreateDto, PortfolioItem>();

        // UserDevice
        CreateMap<UserDevice, UserDeviceDto>();
        CreateMap<UserDeviceCreateDto, UserDevice>();

        // CourseEnrollment
        CreateMap<CourseEnrollment, CourseEnrollmentDto>();
        CreateMap<CourseEnrollmentCreateDto, CourseEnrollment>();

        // PortfolioImage
        CreateMap<PortfolioImage, PortfolioImageDto>();
    }
}
