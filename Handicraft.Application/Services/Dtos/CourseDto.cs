using System;
using System.Collections.Generic;

namespace Handicraft.Application.Services.Dtos;

public class CourseDto
{
    public Guid Id { get; set; }


    public string Title { get; set; } = string.Empty;


    public string Description { get; set; } = string.Empty;


    public string? CoverImageUrl { get; set; }


    public decimal Price { get; set; }


    public bool IsFree { get; set; }


    public bool IsPublished { get; set; }


    public string Level { get; set; } = string.Empty;


    public string Category { get; set; } = string.Empty;


    public int DurationMinutes { get; set; }


    public List<CourseSectionDto> Sections { get; set; }
        = new();
}