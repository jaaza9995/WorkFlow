using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Data;
using WorkFlow.Api.DTOs.Auth;
using WorkFlow.Api.DTOs.User;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokens;

    public AuthService(AppDbContext context, TokenService tokens)
    {
        _context = context;
        _tokens = tokens;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        // Samme svar enten e-posten eller passordet er feil, så man ikke kan lete etter gyldige e-poster
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> RegisterCompanyAsync(RegisterCompanyDto dto)
    {
        var email = dto.Email.Trim().ToLower();
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
            throw new ArgumentException("E-posten er allerede i bruk.");

        var company = new Company
        {
            Name = dto.CompanyName,
            CreatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email.Trim(),
            Role = UserRole.Administrator,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Company = company
        };

        _context.Users.Add(user); // Company lagres sammen med brukeren
        await _context.SaveChangesAsync();

        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user)
    {
        var (token, expiresAt) = _tokens.CreateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                CompanyId = user.CompanyId,
                CustomerId = user.CustomerId
            }
        };
    }
}