using Handicraft.Application.Services.Dtos;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Security.Claims;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AnalyticsController(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // ثبت بازدید صفحه
    // ============================================================

    [HttpPost("page-view")]
    [AllowAnonymous]
    public async Task<IActionResult> AddPageView(
        [FromBody] PageViewRequest request)
    {
        var path = (request.Path ?? "/").Trim();

        if (path.Length == 0)
            path = "/";

        if (path.Length > 300)
            path = path[..300];

        var connection = await GetConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            @"INSERT INTO ""PageViews"" (""Id"",""Path"",""UserId"",""VisitedAt"")
              VALUES (@id, @path, @user, now())";

        AddParameter(command, "@id", Guid.NewGuid());
        AddParameter(command, "@path", path);
        AddParameter(command, "@user", GetUserId());

        await command.ExecuteNonQueryAsync();

        return NoContent();
    }

    // ============================================================
    // ثبت بازدید ویدیو
    // ============================================================

    [HttpPost("video-view")]
    [AllowAnonymous]
    public async Task<IActionResult> AddVideoView(
        [FromBody] VideoViewRequest request)
    {
        if (request.LessonId == Guid.Empty ||
            request.EpisodeId == Guid.Empty)
            return BadRequest();

        var connection = await GetConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            @"INSERT INTO ""VideoViews"" (""Id"",""LessonId"",""EpisodeId"",""UserId"",""ViewedAt"")
              VALUES (@id, @lesson, @episode, @user, now())";

        AddParameter(command, "@id", Guid.NewGuid());
        AddParameter(command, "@lesson", request.LessonId);
        AddParameter(command, "@episode", request.EpisodeId);
        AddParameter(command, "@user", GetUserId());

        await command.ExecuteNonQueryAsync();

        return NoContent();
    }

    // ============================================================
    // خلاصه آمار برای ادمین
    // ============================================================

    [HttpGet("summary")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetSummary()
    {
        var connection = await GetConnectionAsync();

        var summary = new AnalyticsSummaryDto
        {
            TotalPageViews = await CountAllPageViewsAsync(connection),
            PageViewsToday =
                await CountAsync(connection, "PageViews", "today"),

            PageViews7Days =
                await CountAsync(connection, "PageViews", "7days"),

            PageViews30Days =
                await CountAsync(connection, "PageViews", "30days"),

            VideoViewsToday =
                await CountAsync(connection, "VideoViews", "today"),

            VideoViews7Days =
                await CountAsync(connection, "VideoViews", "7days"),

            VideoViews30Days =
                await CountAsync(connection, "VideoViews", "30days"),

            UniqueVisitors30Days =
                await CountUniqueVisitorsAsync(connection),

            TopPages =
                await ReadTopPagesAsync(connection),

            TopVideos =
                await ReadTopVideosAsync(connection)
        };

        return Ok(summary);
    }

    private static async Task<int> CountAllPageViewsAsync(DbConnection connection)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT COUNT(*) FROM ""PageViews""";
        var result = await command.ExecuteScalarAsync();
        return result is null ? 0 : Convert.ToInt32(result);
    }

    // ============================================================
    // Helpers
    // ============================================================

    private Guid? GetUserId()
    {
        var value =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(value, out var id) ? id : null;
    }

    private async Task<DbConnection> GetConnectionAsync()
    {
        var connection = _db.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        return connection;
    }

    private static void AddParameter(
        DbCommand command,
        string name,
        object? value)
    {
        var parameter = command.CreateParameter();

        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;

        command.Parameters.Add(parameter);
    }

    private static string DateFilter(string table, string range)
    {
        var column = table == "PageViews" ? "VisitedAt" : "ViewedAt";

        return range switch
        {
            "today" =>
                $"(\"{column}\" AT TIME ZONE 'Asia/Tehran')::date = " +
                "(now() AT TIME ZONE 'Asia/Tehran')::date",

            "7days" =>
                $"\"{column}\" >= now() - interval '7 days'",

            _ =>
                $"\"{column}\" >= now() - interval '30 days'"
        };
    }

    private static async Task<int> CountAsync(
        DbConnection connection,
        string table,
        string range)
    {
        await using var command = connection.CreateCommand();

        command.CommandText =
            $@"SELECT COUNT(*) FROM ""{table}"" WHERE {DateFilter(table, range)}";

        var result = await command.ExecuteScalarAsync();

        return result is null ? 0 : Convert.ToInt32(result);
    }

    private static async Task<int> CountUniqueVisitorsAsync(
        DbConnection connection)
    {
        await using var command = connection.CreateCommand();

        command.CommandText =
            @"SELECT COUNT(DISTINCT ""UserId"") FROM ""PageViews""
              WHERE ""UserId"" IS NOT NULL
                AND ""VisitedAt"" >= now() - interval '30 days'";

        var result = await command.ExecuteScalarAsync();

        return result is null ? 0 : Convert.ToInt32(result);
    }

    private static async Task<List<AnalyticsItemDto>> ReadTopPagesAsync(
        DbConnection connection)
    {
        await using var command = connection.CreateCommand();

        command.CommandText =
            @"SELECT ""Path"", COUNT(*) AS c
              FROM ""PageViews""
              WHERE ""VisitedAt"" >= now() - interval '30 days'
              GROUP BY ""Path""
              ORDER BY c DESC
              LIMIT 10";

        var items = new List<AnalyticsItemDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new AnalyticsItemDto
            {
                Label = reader.GetString(0),
                Count = Convert.ToInt32(reader.GetValue(1))
            });
        }

        return items;
    }

    private static async Task<List<AnalyticsItemDto>> ReadTopVideosAsync(
        DbConnection connection)
    {
        await using var command = connection.CreateCommand();

        command.CommandText =
            @"SELECT v.""EpisodeId"",
                     COUNT(*) AS c,
                     COALESCE(e.""Title"", '') AS episode_title,
                     COALESCE(c.""Title"", '') AS course_title
              FROM ""VideoViews"" v
              LEFT JOIN ""CourseEpisodes"" e ON e.""Id"" = v.""EpisodeId""
              LEFT JOIN ""CourseLessons"" l ON l.""Id"" = v.""LessonId""
              LEFT JOIN ""CourseSections"" s ON s.""Id"" = l.""SectionId""
              LEFT JOIN ""Courses"" c ON c.""Id"" = s.""CourseId""
              WHERE v.""ViewedAt"" >= now() - interval '30 days'
              GROUP BY v.""EpisodeId"", e.""Title"", c.""Title""
              ORDER BY c DESC
              LIMIT 10";

        var items = new List<AnalyticsItemDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var episodeTitle = reader.GetString(2);
            var courseTitle = reader.GetString(3);

            items.Add(new AnalyticsItemDto
            {
                Label = string.IsNullOrWhiteSpace(episodeTitle)
                    ? "ویدیوی حذف‌شده"
                    : episodeTitle,

                Subtitle = string.IsNullOrWhiteSpace(courseTitle)
                    ? null
                    : courseTitle,

                Count = Convert.ToInt32(reader.GetValue(1))
            });
        }

        return items;
    }
}

public sealed class PageViewRequest
{
    public string? Path { get; set; }
}

public sealed class VideoViewRequest
{
    public Guid LessonId { get; set; }

    public Guid EpisodeId { get; set; }
}
