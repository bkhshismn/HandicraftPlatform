using Handicraft.Application.Services.Dtos;
using Handicraft.Infrastructure.Persistence.Data;
using Handicraft.Api.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Security.Claims;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private const string AdminSenderName = "پشتیبانی مرزی";

    private readonly AppDbContext _db;
    private readonly TelegramEventQueue _telegramEvents;

    public ChatController(AppDbContext db, TelegramEventQueue telegramEvents)
    {
        _db = db;
        _telegramEvents = telegramEvents;
    }

    // ============================================================
    // ارسال پیام کاربر (مهمان یا واردشده)
    // ============================================================

    [HttpPost("messages")]
    [AllowAnonymous]
    public async Task<IActionResult> Send(
        [FromBody] SendChatMessageRequest request)
    {
        var body = (request.Body ?? string.Empty).Trim();

        if (body.Length == 0)
            return BadRequest(new { message = "متن پیام خالی است." });

        if (body.Length > 2000)
            body = body[..2000];

        var conversationId =
            request.ConversationId == Guid.Empty
                ? Guid.NewGuid()
                : request.ConversationId;

        var userId = GetUserId();

        if (userId is not null && !User.IsInRole("Admin") &&
            !await CanUseConversationAsync(conversationId, userId.Value))
            return NotFound();

        var name = (request.Name ?? string.Empty).Trim();

        if (name.Length < 2)
            return BadRequest(new { message = "نام خود را وارد کنید." });

        if (name.Length > 100)
            name = name[..100];

        var phone = NormalizePhone(request.Phone);

        if (phone.Length != 11 || !phone.StartsWith("09", StringComparison.Ordinal))
            return BadRequest(new { message = "شمارهٔ موبایل معتبر وارد کنید." });

        if (phone.Length > 20)
            phone = phone[..20];

        var connection = await GetConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            @"INSERT INTO ""ChatMessages""
                (""Id"",""ConversationId"",""UserId"",""SenderRole"",""SenderName"",""Body"",""Phone"",""CreatedAt"")
              VALUES (@id, @conv, @user, 'User', @name, @body, @phone, now())";

        AddParameter(command, "@id", Guid.NewGuid());
        AddParameter(command, "@conv", conversationId);
        AddParameter(command, "@user", userId);
        AddParameter(command, "@name", name);
        AddParameter(command, "@body", body);
        AddParameter(command, "@phone", string.IsNullOrWhiteSpace(phone) ? null : phone);

        await command.ExecuteNonQueryAsync();

        var senderType = userId is null ? "مهمان" : "کاربر ثبت‌نام‌شده";
        _telegramEvents.TryEnqueue(
            $"📩 پیام جدید سایت\nفرستنده: {senderType}\nنام: {name}\nموبایل: {phone}\nشناسهٔ گفتگو: {conversationId}\n\n{body}");

        return Ok(new { conversationId });
    }

    private static string NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var digits = new string(value
            .Where(char.IsDigit)
            .Select(character => (char)('0' + char.GetNumericValue(character)))
            .ToArray());

        if (digits.StartsWith("0098", StringComparison.Ordinal))
            digits = digits[2..];

        if (digits.StartsWith("98") && digits.Length == 12)
            digits = "0" + digits[2..];

        return digits;
    }

    // ============================================================
    // پیام‌های یک گفتگو
    // ============================================================

    [HttpGet("messages/{conversationId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(
        Guid conversationId)
    {
        var userId = GetUserId();
        if (userId is not null && !User.IsInRole("Admin") &&
            !await CanUseConversationAsync(conversationId, userId.Value))
            return NotFound();

        var connection = await GetConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            @"SELECT ""Id"",""ConversationId"",""SenderRole"",""SenderName"",""Body"",""CreatedAt""
              FROM ""ChatMessages""
              WHERE ""ConversationId"" = @conv
              ORDER BY ""CreatedAt""";

        AddParameter(command, "@conv", conversationId);

        var messages = new List<ChatMessageDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            messages.Add(new ChatMessageDto
            {
                Id = reader.GetGuid(0),
                ConversationId = reader.GetGuid(1),
                SenderRole = reader.GetString(2),
                SenderName = reader.GetString(3),
                Body = reader.GetString(4),
                CreatedAt = reader.GetDateTime(5)
            });
        }

        return Ok(messages);
    }

    private async Task<bool> CanUseConversationAsync(Guid conversationId, Guid userId)
    {
        var connection = await GetConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT NOT EXISTS (
                  SELECT 1 FROM ""ChatMessages"" WHERE ""ConversationId"" = @conv
              ) OR EXISTS (
                  SELECT 1 FROM ""ChatMessages""
                  WHERE ""ConversationId"" = @conv AND ""UserId"" = @user
              )";
        AddParameter(command, "@conv", conversationId);
        AddParameter(command, "@user", userId);

        return Convert.ToBoolean(await command.ExecuteScalarAsync());
    }

    // ============================================================
    // فهرست گفتگوها برای مدیر
    // ============================================================

    [HttpGet("conversations")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ChatConversationDto>>> GetConversations()
    {
        var connection = await GetConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            @"SELECT m.""ConversationId"",
                     u.""SenderName"",
                     m.""Body"",
                     u.""SenderRole"",
                     m.""CreatedAt"",
                     (SELECT COUNT(*) FROM ""ChatMessages"" x
                       WHERE x.""ConversationId"" = m.""ConversationId"") AS c,
                     u.""Phone"",
                     u.""UserId""
              FROM ""ChatMessages"" m
              INNER JOIN (
                  SELECT ""ConversationId"", MAX(""CreatedAt"") AS last_at
                  FROM ""ChatMessages""
                  GROUP BY ""ConversationId""
              ) t ON t.""ConversationId"" = m.""ConversationId""
                 AND t.last_at = m.""CreatedAt""
              INNER JOIN LATERAL (
                  SELECT ""SenderName"", ""SenderRole"", ""Phone"", ""UserId""
                  FROM ""ChatMessages""
                  WHERE ""ConversationId"" = m.""ConversationId""
                    AND ""SenderRole"" = 'User'
                  ORDER BY ""CreatedAt"" ASC
                  LIMIT 1
              ) u ON TRUE
              ORDER BY m.""CreatedAt"" DESC
              LIMIT 100";

        var items = new List<ChatConversationDto>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new ChatConversationDto
            {
                ConversationId = reader.GetGuid(0),
                SenderName = reader.GetString(1),
                LastMessage = reader.GetString(2),
                LastSenderRole = reader.GetString(3),
                LastAt = reader.GetDateTime(4),
                MessageCount = Convert.ToInt32(reader.GetValue(5)),
                Phone = reader.IsDBNull(6) ? null : reader.GetString(6),
                IsGuest = reader.IsDBNull(7)
            });
        }

        return Ok(items);
    }

    // ============================================================
    // پاسخ مدیر
    // ============================================================

    [HttpPost("reply")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reply(
        [FromBody] ReplyChatRequest request)
    {
        var body = (request.Body ?? string.Empty).Trim();

        if (body.Length == 0 || request.ConversationId == Guid.Empty)
            return BadRequest(new { message = "متن پاسخ خالی است." });

        if (body.Length > 2000)
            body = body[..2000];

        var connection = await GetConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            @"INSERT INTO ""ChatMessages""
                (""Id"",""ConversationId"",""UserId"",""SenderRole"",""SenderName"",""Body"",""CreatedAt"")
              VALUES (@id, @conv, NULL, 'Admin', @name, @body, now())";

        AddParameter(command, "@id", Guid.NewGuid());
        AddParameter(command, "@conv", request.ConversationId);
        AddParameter(command, "@name", AdminSenderName);
        AddParameter(command, "@body", body);

        await command.ExecuteNonQueryAsync();

        return NoContent();
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
}

public sealed class SendChatMessageRequest
{
    public Guid ConversationId { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Body { get; set; }
}

public sealed class ReplyChatRequest
{
    public Guid ConversationId { get; set; }

    public string? Body { get; set; }
}
