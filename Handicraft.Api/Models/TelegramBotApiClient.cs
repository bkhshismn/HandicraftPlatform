using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Handicraft.Api.Notifications;

public sealed class TelegramBotApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _token;

    public TelegramBotApiClient(IHttpClientFactory httpClientFactory, IOptions<TelegramBotOptions> options)
    {
        _httpClient = httpClientFactory.CreateClient("TelegramBotApi");
        _token = options.Value.BotToken.Trim();
    }

    public async Task<IReadOnlyList<TelegramUpdate>> GetUpdatesAsync(long offset, CancellationToken cancellationToken)
    {
        var path = $"/bot{_token}/getUpdates?offset={offset}&timeout=25&allowed_updates=%5B%22message%22%5D";
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<TelegramApiResult<List<TelegramUpdate>>>(cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode || result?.Ok != true)
            throw new HttpRequestException($"Telegram getUpdates failed ({(int)response.StatusCode}): {result?.Description ?? response.ReasonPhrase}");

        return result.Result ?? [];
    }

    public async Task SendMessageAsync(long chatId, string text, object? replyMarkup, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"/bot{_token}/sendMessage",
            new TelegramSendMessage(chatId, text, replyMarkup),
            cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<TelegramApiResult<JsonElement>>(cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode || result?.Ok != true)
            throw new HttpRequestException($"Telegram sendMessage failed ({(int)response.StatusCode}): {result?.Description ?? response.ReasonPhrase}");
    }

    private sealed record TelegramSendMessage(
        [property: JsonPropertyName("chat_id")] long ChatId,
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("reply_markup")] object? ReplyMarkup);
}

public sealed class TelegramApiResult<T>
{
    [JsonPropertyName("ok")] public bool Ok { get; set; }
    [JsonPropertyName("result")] public T? Result { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
}

public sealed class TelegramUpdate
{
    [JsonPropertyName("update_id")] public long UpdateId { get; set; }
    [JsonPropertyName("message")] public TelegramMessage? Message { get; set; }
}

public sealed class TelegramMessage
{
    [JsonPropertyName("chat")] public TelegramChat? Chat { get; set; }
    [JsonPropertyName("from")] public TelegramUser? From { get; set; }
    [JsonPropertyName("text")] public string? Text { get; set; }
    [JsonPropertyName("contact")] public TelegramContact? Contact { get; set; }
}

public sealed class TelegramChat
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
}

public sealed class TelegramUser
{
    [JsonPropertyName("id")] public long Id { get; set; }
}

public sealed class TelegramContact
{
    [JsonPropertyName("phone_number")] public string PhoneNumber { get; set; } = string.Empty;
    [JsonPropertyName("user_id")] public long? UserId { get; set; }
}
