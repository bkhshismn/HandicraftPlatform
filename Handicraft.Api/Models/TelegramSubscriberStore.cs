using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Handicraft.Api.Notifications;

public sealed class TelegramSubscriberStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public TelegramSubscriberStore(IOptions<TelegramBotOptions> options)
    {
        _filePath = Path.Combine(options.Value.DataDirectory, "subscribers.json");
    }

    public async Task<IReadOnlyList<TelegramSubscriber>> GetAllAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await ReadUnsafeAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> IsRegisteredAsync(long chatId, CancellationToken cancellationToken) =>
        (await GetAllAsync(cancellationToken)).Any(subscriber => subscriber.ChatId == chatId);

    public async Task SaveAsync(int recipientIndex, long chatId, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var subscribers = (await ReadUnsafeAsync(cancellationToken))
                .Where(subscriber => subscriber.ChatId != chatId && subscriber.RecipientIndex != recipientIndex)
                .ToList();
            subscribers.Add(new TelegramSubscriber(recipientIndex, chatId));
            await WriteUnsafeAsync(subscribers, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RemoveAsync(long chatId, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var subscribers = (await ReadUnsafeAsync(cancellationToken))
                .Where(subscriber => subscriber.ChatId != chatId)
                .ToList();
            await WriteUnsafeAsync(subscribers, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<TelegramSubscriber>> ReadUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
            return [];

        await using var input = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<TelegramSubscriber>>(
                   input,
                   _jsonOptions,
                   cancellationToken)
               ?? [];
    }

    private async Task WriteUnsafeAsync(List<TelegramSubscriber> subscribers, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(directory);
        if (OperatingSystem.IsLinux())
            File.SetUnixFileMode(directory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        var temporaryPath = _filePath + ".tmp";
        await using (var output = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(output, subscribers, _jsonOptions, cancellationToken);
        }

        if (OperatingSystem.IsLinux())
            File.SetUnixFileMode(temporaryPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        File.Move(temporaryPath, _filePath, overwrite: true);
    }
}

public sealed record TelegramSubscriber(int RecipientIndex, long ChatId);
