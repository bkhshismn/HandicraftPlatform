using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Handicraft.Domain.Entities;
using BCrypt.Net;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;

    public AuthController(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration, AppDbContext db)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
        _db = db;
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshRequestDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Token)) return BadRequest();
        if (_refreshTokenRepository == null) return StatusCode(500, new { message = "Refresh token repository not available" });

        var stored = await _refreshTokenRepository.GetByTokenAsync(dto.Token);
        if (stored == null) return NotFound();

        await _refreshTokenRepository.RevokeAsync(stored);
        return NoContent();
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        if (_refreshTokenRepository == null) return StatusCode(500, new { message = "Refresh token repository not available" });

        await _refreshTokenRepository.RevokeAllForUserAsync(userId);
        return NoContent();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (dto == null) return BadRequest();
        if (string.IsNullOrWhiteSpace(dto.Phone) || dto.Phone.Length > 50 ||
            string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 100)
            return BadRequest(new { message = "نام و شماره موبایل معتبر وارد کنید." });
        // basic validation
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters." });

        var unique = await _userRepository.IsPhoneUniqueAsync(dto.Phone);
        if (!unique) return Conflict(new { message = "این شمارهٔ موبایل قبلاً ثبت‌نام کرده است. با این شماره وارد شوید." });

        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Phone = dto.Phone,
            Username = dto.Phone,
            Name = dto.Name,

            Role = "User",

            IsActive = false,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _userRepository.AddAsync(user);
        // if repository does not save changes, ensure caller saves; our GenericRepository uses context and SaveChanges may be required.
        // Attempt to save via a minimal approach if repository exposes no SaveChanges: rely on unit of work not present � use context via cast if needed.

        return Accepted(new { message = "Registration received and awaiting administrator approval." });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(
            new ProfileDto(
                user.Id,
                user.Name,
                user.Phone,
                user.Role,
                user.IsActive,
                user.CreatedAt));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "Name is required." });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound();

        user.Name = name;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (dto == null) return BadRequest();

        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound();

        var verified = BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash);
        if (!verified) return BadRequest(new { message = "Old password is incorrect" });

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
            return BadRequest(new { message = "New password must be at least 8 characters." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);

        // revoke existing refresh tokens for safety
        if (_refreshTokenRepository != null)
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(user.Id);
        }

        return NoContent();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(userIdClaim, out userId);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        // refresh using persistent refresh tokens
        if (dto == null || string.IsNullOrWhiteSpace(dto.Token)) return BadRequest();
        if (_refreshTokenRepository == null) return StatusCode(500, new { message = "Refresh token repository not available" });

        var stored = await _refreshTokenRepository.GetByTokenAsync(dto.Token);
        if (stored == null)
            return Unauthorized();

        if (stored.IsRevoked)
            return Unauthorized(new
            {
                message = "Refresh token has been revoked"
            });

        if (stored.ExpiresAt <= DateTime.UtcNow)
            return Unauthorized(new
            {
                message = "Refresh token expired"
            });

        var user = await _userRepository.GetByIdAsync(stored.UserId);
        if (user == null) return Unauthorized();

        var newRefresh = CreateRefreshToken(user.Id);

        // Atomically claim the old token before inserting its replacement.
        // Concurrent refresh requests can only update the active row once.
        await using (var transaction = await _db.Database.BeginTransactionAsync())
        {
            var now = DateTime.UtcNow;
            var claimed = await _db.RefreshTokens
                .Where(x => x.Id == stored.Id &&
                            !x.IsRevoked &&
                            x.ExpiresAt > now)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.IsRevoked, true)
                    .SetProperty(x => x.ReplacedByToken, newRefresh.Token));

            if (claimed != 1)
            {
                await transaction.RollbackAsync();
                return Unauthorized();
            }

            await _refreshTokenRepository.AddAsync(newRefresh);
            await transaction.CommitAsync();
        }

        var newAccess = GenerateToken(user);
        return Ok(new AuthResponseDto { Token = newAccess, ExpiresInMinutes = _configuration.GetValue<int>("Jwt:ExpiresMinutes"), RefreshToken = newRefresh.Token });
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto == null) return BadRequest();

        var loginName = string.IsNullOrWhiteSpace(dto.Username) ? dto.Phone : dto.Username;
        var user = await _userRepository.GetByUsernameAsync(loginName);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var verified = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!verified) return Unauthorized(new { message = "Invalid credentials" });

        if (!user.IsApproved)
        {
            return Unauthorized(new
            {
                message = "درخواست ثبت‌نام شما برای تأیید دسترسی به مدیر ارسال شده است. پس از تأیید می‌توانید وارد شوید."
            });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new
            {
                message = "حساب کاربری شما مسدود است. برای رفع انسداد با مدیر تماس بگیرید."
            });
        }
        if (!await RecordLoginIpAsync(user.Id, enforceDeviceLimit: user.Role != "Admin"))
        {
            return Unauthorized(new
            {
                message = "برای هر حساب حداکثر دو دستگاه مجاز است. دسترسی حساب شما مسدود شد؛ با مدیر تماس بگیرید."
            });
        }

        var token = GenerateToken(user);
        // create refresh token
        var refresh = CreateRefreshToken(user.Id);
        if (_refreshTokenRepository != null)
        {
            await _refreshTokenRepository.AddAsync(refresh);
        }

        return Ok(new AuthResponseDto { Token = token, ExpiresInMinutes = _configuration.GetValue<int>("Jwt:ExpiresMinutes"), RefreshToken = refresh.Token });
    }

    private async Task<bool> RecordLoginIpAsync(Guid userId, bool enforceDeviceLimit)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(ip)) return true;
        if (ip.Length > 50) ip = ip[..50];

        var device = await _db.UserDevices.FirstOrDefaultAsync(x => x.UserId == userId && x.IpAddress == ip);
        var activeIpCount = await _db.UserDevices.CountAsync(x => x.UserId == userId && x.IsActive);
        var isNewAllowedIp = device is null || !device.IsActive;
        var exceedsLimit = enforceDeviceLimit && isNewAllowedIp && activeIpCount >= 2;
        var now = DateTime.UtcNow;
        if (device is null)
        {
            var agent = Request.Headers.UserAgent.ToString();
            _db.UserDevices.Add(new UserDevice
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeviceId = $"ip:{ip}"[..Math.Min($"ip:{ip}".Length, 100)],
                DeviceName = string.IsNullOrWhiteSpace(agent) ? "نامشخص" : agent[..Math.Min(agent.Length, 200)],
                IpAddress = ip,
                LastSeenAt = now,
                IsActive = !exceedsLimit,
                CreatedAt = now
            });
        }
        else
        {
            device.LastSeenAt = now;
            var agent = Request.Headers.UserAgent.ToString();
            if (!string.IsNullOrWhiteSpace(agent)) device.DeviceName = agent[..Math.Min(agent.Length, 200)];
            if (!exceedsLimit) device.IsActive = true;
        }

        if (exceedsLimit)
        {
            var account = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (account is not null)
            {
                account.IsActive = false;
                account.UpdatedAt = now;
            }

            var tokens = await _db.RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync();
            foreach (var token in tokens) token.IsRevoked = true;
        }

        await _db.SaveChangesAsync();
        return !exceedsLimit;
    }

    private string GenerateToken(Domain.Entities.User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key");
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("JWT signing key is not configured.");
        var issuer = jwtSection.GetValue<string>("Issuer");
        var audience = jwtSection.GetValue<string>("Audience");
        var expires = jwtSection.GetValue<int>("ExpiresMinutes");

        var role = user.Role ?? string.Empty;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("phone", user.Phone ?? string.Empty),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role),
            new Claim("roles", role),
            new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role)
        };

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expires > 0 ? expires : 60),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private RefreshToken CreateRefreshToken(Guid userId)
    {
        var random = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(random);
        var token = Convert.ToBase64String(random);

        var expiresDays = _configuration.GetValue<int>("Jwt:RefreshExpiresDays", 7);

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expiresDays),
            IsRevoked = false
        };
    }
}
