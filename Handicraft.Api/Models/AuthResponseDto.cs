namespace Handicraft.Api.Models;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; }
    public string? RefreshToken { get; set; }
}
