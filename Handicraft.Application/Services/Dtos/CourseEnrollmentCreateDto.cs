namespace Handicraft.Application.Services.Dtos;

public record CourseEnrollmentCreateDto(
    Guid UserId,
    Guid CourseId,
    decimal PaidAmount,
    DateTime? ExpiresAt
);
