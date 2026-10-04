using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Data;
using WorkFlow.Api.DTOs.User;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role,
                CompanyId = u.CompanyId,
                CustomerId = u.CustomerId
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return null;

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            CompanyId = user.CompanyId,
            CustomerId = user.CustomerId
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        if (dto.Role == UserRole.Customer && dto.CustomerId is null)
            throw new ArgumentException("Kundebrukere må ha en CustomerId.");

        if (dto.Role != UserRole.Customer && dto.CustomerId is not null)
            throw new ArgumentException("Kun kundebrukere kan ha en CustomerId.");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Role = dto.Role,
            CompanyId = dto.CompanyId,
            CustomerId = dto.CustomerId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            CompanyId = user.CompanyId,
            CustomerId = user.CustomerId
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return false;

        if (dto.Role == UserRole.Customer && dto.CustomerId is null)
            throw new ArgumentException("Kundebrukere må ha en CustomerId.");

        if (dto.Role != UserRole.Customer && dto.CustomerId is not null)
            throw new ArgumentException("Kun kundebrukere kan ha en CustomerId.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.Role = dto.Role;
        user.CustomerId = dto.CustomerId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}