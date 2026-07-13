namespace Handicraft.Domain.Entities;

public class CourseLessonProgress : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid LessonId { get; set; }


    public bool Completed { get; set; }


    public int WatchedSeconds { get; set; }


    public User User { get; set; } = null!;

    public CourseLesson Lesson { get; set; } = null!;
}