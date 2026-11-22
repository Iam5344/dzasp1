using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManager.Data;
using UserManager.DTOs;
using UserManager.Models;
using UserManager.Services;

namespace UserManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthController(AppDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto, CancellationToken cancellationToken)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var exists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (exists)
        {
            return Conflict("Користувач з таким email вже існує.");
        }

        var user = new User
        {
            Username = dto.Username.Trim(),
            Email = normalizedEmail,
            Role = "User",
            IsBlocked = false,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Unauthorized("Невірний email або пароль.");
        }

        if (user.IsBlocked)
        {
            return Forbid();
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Невірний email або пароль.");
        }

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
    }
}
