namespace Handicraft.Api.Notifications;

public sealed class TelegramBotOptions
{
    public string BotToken { get; set; } = string.Empty;
    public string AllowedPhoneNumbers { get; set; } = string.Empty;
    public string DataDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "TelegramData");

    public IReadOnlyList<string> GetAllowedPhones() => AllowedPhoneNumbers
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(TelegramPhone.Normalize)
        .Where(phone => phone.Length > 0)
        .Distinct(StringComparer.Ordinal)
        .ToArray();
}

internal static class TelegramPhone
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0098", StringComparison.Ordinal))
            digits = digits[2..];
        else if (digits.StartsWith("0", StringComparison.Ordinal))
            digits = "98" + digits[1..];

        if (digits.StartsWith("98", StringComparison.Ordinal))
            return "+" + digits;

        return digits.Length > 0 ? "+" + digits : string.Empty;
    }
}
