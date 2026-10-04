using System.ComponentModel.DataAnnotations;

namespace WorkFlow.Api.DTOs.TimeEntries;

public class TimeEntryDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public int ProjectId { get; set; }
}

public class CreateTimeEntryDto
{
    public DateOnly Date { get; set; }

    [Range(0.01, 24)]
    public decimal Hours { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int TaskId { get; set; }
}

public class UpdateTimeEntryDto
{
    public DateOnly Date { get; set; }

    [Range(0.01, 24)]
    public decimal Hours { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class ProjectHoursDto
{
    public int ProjectId { get; set; }
    public decimal TotalHours { get; set; }
    public List<UserHoursDto> ByUser { get; set; } = new();
}

public class UserHoursDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
}