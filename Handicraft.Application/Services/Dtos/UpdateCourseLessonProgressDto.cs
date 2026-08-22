namespace Handicraft.Application.Services.Dtos;

public record UpdateCourseLessonProgressDto(
    int WatchedSeconds,
    bool Completed);