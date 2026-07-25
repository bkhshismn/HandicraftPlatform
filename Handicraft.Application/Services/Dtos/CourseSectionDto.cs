using System;
using System.Collections.Generic;

namespace Handicraft.Application.Services.Dtos;

public class CourseSectionDto
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }


    public List<CourseLessonDto> Lessons { get; set; }
        = new();
}