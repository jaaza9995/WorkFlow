using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Data;
using WorkFlow.Api.DTOs.TimeEntries;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

public class TimeEntryService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUser _user;

    public TimeEntryService(AppDbContext context, ICurrentUser user)
    {
        _context = context;
        _user = user;
    }

    private static readonly Expression<Func<TimeEntry, TimeEntryDto>> ToDto = te => new TimeEntryDto
    {
        Id = te.Id,
        Date = te.Date,
        Hours = te.Hours,
        Description = te.Description,
        UserId = te.UserId,
        UserName = te.User.FirstName + " " + te.User.LastName,
        TaskId = te.TaskId,
        TaskTitle = te.Task.Title,
        ProjectId = te.Task.ProjectId
    };

    public async Task<List<TimeEntryDto>> GetAllAsync(int? projectId, int? taskId, int? userId)
    {
        var query = _context.TimeEntries.VisibleTo(_user);

        if (projectId.HasValue)
            query = query.Where(te => te.Task.ProjectId == projectId.Value);

        if (taskId.HasValue)
            query = query.Where(te => te.TaskId == taskId.Value);

        if (userId.HasValue)
            query = query.Where(te => te.UserId == userId.Value);

        return await query
            .OrderByDescending(te => te.Date)
            .ThenByDescending(te => te.Id)
            .Select(ToDto)
            .ToListAsync();
    }

    public async Task<TimeEntryDto?> GetByIdAsync(int id)
    {
        return await _context.TimeEntries
            .VisibleTo(_user)
            .Where(te => te.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    public async Task<TimeEntryDto> CreateAsync(CreateTimeEntryDto dto)
    {
        var taskExists = await _context.Tasks.VisibleTo(_user).AnyAsync(t => t.Id == dto.TaskId);
        if (!taskExists)
            throw new ArgumentException("Oppgaven finnes ikke.");

        if (_user.Role == UserRole.Customer)
            throw new ArgumentException("Kundebrukere kan ikke registrere timer.");

        var entry = new TimeEntry
        {
            Date = dto.Date,
            Hours = dto.Hours,
            Description = dto.Description,
            UserId = _user.UserId,
            TaskId = dto.TaskId
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(entry.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateTimeEntryDto dto)
    {
        var entry = await _context.TimeEntries.VisibleTo(_user).FirstOrDefaultAsync(te => te.Id == id);
        if (entry is null) return false;

        if (entry.UserId != _user.UserId)
            throw new UnauthorizedAccessException("Du kan bare endre dine egne timer.");

        entry.Date = dto.Date;
        entry.Hours = dto.Hours;
        entry.Description = dto.Description;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _context.TimeEntries.VisibleTo(_user).FirstOrDefaultAsync(te => te.Id == id);
        if (entry is null) return false;

        if (entry.UserId != _user.UserId)
            throw new UnauthorizedAccessException("Du kan bare endre dine egne timer.");

        _context.TimeEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return true;
    }

    // Totalt antall timer på et prosjekt, og fordelt per bruker (for managere)
    public async Task<ProjectHoursDto?> GetProjectHoursAsync(int projectId)
    {
        if (!await _context.Projects.VisibleTo(_user).AnyAsync(p => p.Id == projectId))
            return null;

        var entries = _context.TimeEntries.Where(te => te.Task.ProjectId == projectId);

        var total = await entries.SumAsync(te => (decimal?)te.Hours) ?? 0m;

        var byUser = await entries
            .GroupBy(te => new { te.UserId, te.User.FirstName, te.User.LastName })
            .Select(g => new UserHoursDto
            {
                UserId = g.Key.UserId,
                UserName = g.Key.FirstName + " " + g.Key.LastName,
                Hours = g.Sum(te => te.Hours)
            })
            .ToListAsync();

        return new ProjectHoursDto
        {
            ProjectId = projectId,
            TotalHours = total,
            ByUser = byUser
        };
    }
}