using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Handicraft.Api.Notifications;

public sealed class TelegramBotPollingService : BackgroundService
{
    private readonly TelegramBotApiClient _telegram;
    private readonly TelegramSubscriberStore _subscribers;
    private readonly TelegramBotOptions _options;
    private readonly ILogger<TelegramBotPollingService> _logger;

    public TelegramBotPollingService(
        TelegramBotApiClient telegram,
        TelegramSubscriberStore subscribers,
        IOptions<TelegramBotOptions> options,
        ILogger<TelegramBotPollingService> logger)
    {
        _telegram = telegram;
        _subscribers = subscribers;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BotToken))
        {
            _logger.LogInformation("Telegram notifications are disabled because no bot token is configured.");
            return;
        }

        var offset = 0L;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _telegram.GetUpdatesAsync(offset, stoppingToken);
                foreach (var update in updates)
                {
                    try
                    {
                        await HandleUpdateAsync(update, stoppingToken);
                    }
                    catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning(exception, "Could not process a Telegram bot update.");
                    }

                    offset = Math.Max(offset, update.UpdateId + 1);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(exception, "Telegram bot polling failed; retrying shortly.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task HandleUpdateAsync(TelegramUpdate update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        var chat = message?.Chat;
        var sender = message?.From;
        if (chat is null || sender is null || chat.Type != "private")
            return;

        if (message!.Text?.StartsWith("/stop", StringComparison.OrdinalIgnoreCase) == true)
        {
            await _subscribers.RemoveAsync(chat.Id, cancellationToken);
            await _telegram.SendMessageAsync(chat.Id, "دریافت اعلان‌ها متوقف شد.", RemoveKeyboard, cancellationToken);
            return;
        }

        if (message.Text?.StartsWith("/start", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (await _subscribers.IsRegisteredAsync(chat.Id, cancellationToken))
            {
                await _telegram.SendMessageAsync(chat.Id, "این حساب برای دریافت اعلان‌های سایت فعال است. برای توقف، /stop را بفرستید.", null, cancellationToken);
                return;
            }

            await _telegram.SendMessageAsync(
                chat.Id,
                "برای فعال‌سازی اعلان‌های سایت، شمارهٔ خودتان را با دکمهٔ زیر به اشتراک بگذارید. شماره فقط برای تطبیق با فهرست مجاز استفاده می‌شود.",
                ContactKeyboard,
                cancellationToken);
            return;
        }

        if (message.Contact is not { } contact)
            return;

        if (contact.UserId != sender.Id)
        {
            await _telegram.SendMessageAsync(chat.Id, "لطفاً شمارهٔ خودتان را با دکمهٔ اشتراک‌گذاری مخاطب ارسال کنید.", ContactKeyboard, cancellationToken);
            return;
        }

        var phone = TelegramPhone.Normalize(contact.PhoneNumber);
        var recipientIndex = _options.GetAllowedPhones().ToList().FindIndex(allowedPhone => allowedPhone == phone);
        if (recipientIndex < 0)
        {
            await _telegram.SendMessageAsync(chat.Id, "این شماره برای دریافت اعلان‌های سایت مجاز نیست.", RemoveKeyboard, cancellationToken);
            return;
        }

        await _subscribers.SaveAsync(recipientIndex, chat.Id, cancellationToken);
        await _telegram.SendMessageAsync(chat.Id, "ثبت شد؛ اعلان‌های مهم سایت برای شما ارسال می‌شوند. برای توقف، /stop را بفرستید.", RemoveKeyboard, cancellationToken);
        _logger.LogInformation("An approved Telegram notification recipient registered.");
    }

    private static readonly object ContactKeyboard = new
    {
        keyboard = new[] { new[] { new { text = "اشتراک‌گذاری شمارهٔ من", request_contact = true } } },
        resize_keyboard = true,
        one_time_keyboard = true
    };

    private static readonly object RemoveKeyboard = new { remove_keyboard = true };
}

public sealed class TelegramEventDispatcher : BackgroundService
{
    private readonly TelegramEventQueue _queue;
    private readonly TelegramSubscriberStore _subscribers;
    private readonly TelegramBotApiClient _telegram;
    private readonly TelegramBotOptions _options;
    private readonly ILogger<TelegramEventDispatcher> _logger;

    public TelegramEventDispatcher(
        TelegramEventQueue queue,
        TelegramSubscriberStore subscribers,
        TelegramBotApiClient telegram,
        IOptions<TelegramBotOptions> options,
        ILogger<TelegramEventDispatcher> logger)
    {
        _queue = queue;
        _subscribers = subscribers;
        _telegram = telegram;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BotToken))
            return;

        await foreach (var message in _queue.ReadAllAsync(stoppingToken))
        {
            var recipients = await _subscribers.GetAllAsync(stoppingToken);
            foreach (var recipient in recipients)
            {
                try
                {
                    await _telegram.SendMessageAsync(recipient.ChatId, message, null, stoppingToken);
                }
                catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(exception, "Could not deliver a Telegram site notification.");
                }
            }
        }
    }
}
