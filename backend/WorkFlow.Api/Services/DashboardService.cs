using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Data;
using WorkFlow.Api.DTOs.Dashboard;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

public class DashboardService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUser _user;
    private readonly ProjectService _projects;
    private readonly TaskService _tasks;

    public DashboardService(AppDbContext context, ICurrentUser user, ProjectService projects, TaskService tasks)
    {
        _context = context;
        _user = user;
        _projects = projects;
        _tasks = tasks;
    }

    public async Task<DashboardDto> GetAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-(((int)today.DayOfWeek + 6) % 7)); // mandag
        var weekEnd = weekStart.AddDays(6);

        // Ansatte ser oppgavene de selv er ansvarlig for, andre ser alt de har tilgang til
        var tasks = _context.Tasks.VisibleTo(_user);
        int? assignedFilter = null;
        if (_user.Role == UserRole.Employee)
        {
            var userId = _user.UserId;
            tasks = tasks.Where(t => t.AssignedToUserId == userId);
            assignedFilter = userId;
        }

        var openTasks = tasks.Where(t => t.Status != WorkTaskStatus.Done);

        var dto = new DashboardDto
        {
            Role = _user.Role,
            ActiveProjects = await _context.Projects.VisibleTo(_user)
                .CountAsync(p => p.Status == ProjectStatus.Active),
            OpenTasks = await openTasks.CountAsync(),
            OverdueTasks = await openTasks.CountAsync(t => t.DueDate != null && t.DueDate < today),
            TasksThisWeek = await openTasks.CountAsync(t =>
                t.DueDate != null && t.DueDate >= weekStart && t.DueDate <= weekEnd)
        };

        if (_user.Role != UserRole.Customer)
        {
            dto.HoursThisWeek = await _context.TimeEntries.VisibleTo(_user)
                .Where(te => te.Date >= weekStart && te.Date <= weekEnd)
                .SumAsync(te => (decimal?)te.Hours) ?? 0m;
        }

        dto.ProjectProgress = (await _projects.GetAllAsync(null, ProjectStatus.Active, null))
            .Take(6)
            .ToList();

        dto.UpcomingTasks = (await _tasks.GetAllAsync(null, null, assignedFilter, null))
            .Where(t => t.Status != WorkTaskStatus.Done)
            .Take(5)
            .ToList();

        return dto;
    }
}