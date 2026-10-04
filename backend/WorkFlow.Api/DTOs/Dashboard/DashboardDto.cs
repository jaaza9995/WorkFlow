using WorkFlow.Api.DTOs.Projects;
using WorkFlow.Api.DTOs.Tasks;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.DTOs.Dashboard;

public class DashboardDto
{
    public UserRole Role { get; set; }
    public int ActiveProjects { get; set; }
    public int OpenTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int TasksThisWeek { get; set; }

    // Null for kunder, som ikke ser timer
    public decimal? HoursThisWeek { get; set; }

    public List<ProjectDto> ProjectProgress { get; set; } = new();
    public List<TaskDto> UpcomingTasks { get; set; } = new();
}