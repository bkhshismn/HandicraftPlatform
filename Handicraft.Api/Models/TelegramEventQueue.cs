using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Handicraft.Api.Notifications;

public sealed class TelegramEventQueue
{
    private readonly Channel<string> _channel = Channel.CreateBounded<string>(
        new BoundedChannelOptions(250)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest
        });
    private readonly bool _enabled;

    public TelegramEventQueue(IOptions<TelegramBotOptions> options) =>
        _enabled = !string.IsNullOrWhiteSpace(options.Value.BotToken);

    public bool TryEnqueue(string message) => _enabled && _channel.Writer.TryWrite(message);

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
