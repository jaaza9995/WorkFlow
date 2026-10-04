using System.ComponentModel.DataAnnotations;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.DTOs.Projects;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public int CompanyId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int ResponsibleManagerId { get; set; }
    public string ResponsibleManagerName { get; set; } = string.Empty;

    // Fremdrift lagres ikke, den regnes ut fra oppgavene
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ProgressPercent =>
        TotalTasks == 0 ? 0 : (int)Math.Round(100.0 * CompletedTasks / TotalTasks);
}

public class CreateProjectDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int ResponsibleManagerId { get; set; }
}

public class UpdateProjectDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public ProjectStatus Status { get; set; }

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int ResponsibleManagerId { get; set; }
}