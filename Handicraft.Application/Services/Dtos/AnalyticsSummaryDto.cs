namespace Handicraft.Application.Services.Dtos;

public sealed class AnalyticsSummaryDto
{
    public int PageViewsToday { get; set; }

    public int PageViews7Days { get; set; }

    public int PageViews30Days { get; set; }

    public int UniqueVisitors30Days { get; set; }

    public int VideoViewsToday { get; set; }

    public int VideoViews7Days { get; set; }

    public int VideoViews30Days { get; set; }

    public List<AnalyticsItemDto> TopPages { get; set; } = [];

    public List<AnalyticsItemDto> TopVideos { get; set; } = [];
}

public sealed class AnalyticsItemDto
{
    public string Label { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    public int Count { get; set; }
}
