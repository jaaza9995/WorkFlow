using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Data;
using WorkFlow.Api.DTOs.Tasks;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

public class TaskService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUser _user;

    public TaskService(AppDbContext context, ICurrentUser user)
    {
        _context = context;
        _user = user;
    }

    private static readonly Expression<Func<WorkTask, TaskDto>> ToDto = t => new TaskDto
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        Priority = t.Priority,
        DueDate = t.DueDate,
        CreatedAt = t.CreatedAt,
        ProjectId = t.ProjectId,
        ProjectName = t.Project.Name,
        AssignedToUserId = t.AssignedToUserId,
        AssignedToUserName = t.AssignedToUser == null
            ? null
            : t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName
    };

    public async Task<List<TaskDto>> GetAllAsync(
        int? projectId, WorkTaskStatus? status, int? assignedToUserId, string? search)
    {
        var query = _context.Tasks.VisibleTo(_user);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (assignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => EF.Functions.ILike(t.Title, $"%{search}%"));

        return await query
            .OrderBy(t => t.DueDate)
            .ThenBy(t => t.Id)
            .Select(ToDto)
            .ToListAsync();
    }

    public async Task<TaskDto?> GetByIdAsync(int id)
    {
        return await _context.Tasks
            .VisibleTo(_user)
            .Where(t => t.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto)
    {
        if (!await _context.Projects.VisibleTo(_user).AnyAsync(p => p.Id == dto.ProjectId))
            throw new ArgumentException("Prosjektet finnes ikke.");

        await ValidateAssigneeAsync(dto.ProjectId, dto.AssignedToUserId);

        var task = new WorkTask
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = WorkTaskStatus.NotStarted,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            ProjectId = dto.ProjectId,
            AssignedToUserId = dto.AssignedToUserId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(task.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateTaskDto dto)
    {
        var task = await _context.Tasks.VisibleTo(_user).FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return false;

        await ValidateAssigneeAsync(task.ProjectId, dto.AssignedToUserId);

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.AssignedToUserId = dto.AssignedToUserId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int id, WorkTaskStatus status)
    {
        var task = await _context.Tasks.VisibleTo(_user).FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return false;

        // Ansatte kan bare endre status på oppgaver de selv er ansvarlig for
        if (_user.Role == UserRole.Employee && task.AssignedToUserId != _user.UserId)
            throw new UnauthorizedAccessException("Du kan bare endre status på egne oppgaver.");

        task.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.Tasks.VisibleTo(_user).FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return false;

        var hasRelatedData =
            await _context.Comments.AnyAsync(c => c.TaskId == id) ||
            await _context.TimeEntries.AnyAsync(te => te.TaskId == id);

        if (hasRelatedData)
            throw new InvalidOperationException("Oppgaven har kommentarer eller timer og kan ikke slettes.");

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    // Den ansvarlige må være med i prosjektet (ProjectUsers)
    private async Task ValidateAssigneeAsync(int projectId, int? userId)
    {
        if (userId is null) return;

        var isMember = await _context.ProjectUsers
            .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId.Value);

        if (!isMember)
            throw new ArgumentException("Ansvarlig må være tilknyttet prosjektet.");
    }
}