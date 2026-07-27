public class AuthResponseDto
{
    public string Token { get; set; } = "";

    public int ExpiresInMinutes { get; set; }

    public string? RefreshToken { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Role { get; set; } = "";
}