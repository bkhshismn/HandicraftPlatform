namespace Handicraft.Application.Services.Dtos;

public record CourseEnrollmentDto(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    decimal PaidAmount,
    DateTime EnrolledAt,
    DateTime? ExpiresAt,
    bool IsActive
);
