using AutoMapper;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;

namespace Handicraft.Application.Services;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Course
        CreateMap<Course, CourseDto>();
        CreateMap<CourseEpisode, CourseEpisodeDto>();
        CreateMap<CourseCreateDto, Course>();
        CreateMap<CourseEpisodeCreateDto, CourseEpisode>();

        // User
        CreateMap<User, UserDto>();
        CreateMap<UserCreateDto, User>();

        // Order
        CreateMap<Order, OrderDto>();
        CreateMap<OrderCreateDto, Order>();

        // Portfolio
        CreateMap<PortfolioItem, PortfolioItemDto>();
        CreateMap<PortfolioCreateDto, PortfolioItem>();

        // UserDevice
        CreateMap<UserDevice, UserDeviceDto>();
        CreateMap<UserDeviceCreateDto, UserDevice>();

        // CourseEnrollment
        CreateMap<CourseEnrollment, CourseEnrollmentDto>();
        CreateMap<CourseEnrollmentCreateDto, CourseEnrollment>();
    }
}
