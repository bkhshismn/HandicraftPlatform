namespace Handicraft.Domain.Entities;

public class CourseSection : BaseEntity
{
    public Guid CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }


    public Course Course { get; set; } = null!;


    public ICollection<CourseLesson> Lessons { get; set; }
        = new List<CourseLesson>();
}