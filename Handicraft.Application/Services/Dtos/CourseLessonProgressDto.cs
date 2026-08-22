namespace Handicraft.Application.Services.Dtos;

public record CourseLessonProgressDto(
    Guid LessonId,
    int WatchedSeconds,
    bool Completed);