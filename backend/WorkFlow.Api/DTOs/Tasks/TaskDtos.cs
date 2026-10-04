using System.ComponentModel.DataAnnotations;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.DTOs.Tasks;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkTaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
}

public class CreateTaskDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateOnly? DueDate { get; set; }

    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }

    public int? AssignedToUserId { get; set; }
}

public class UpdateTaskDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public DateOnly? DueDate { get; set; }
    public int? AssignedToUserId { get; set; }
}

// Egen DTO slik at ansatte kan endre status uten å kunne endre resten av oppgaven
public class UpdateTaskStatusDto
{
    public WorkTaskStatus Status { get; set; }
}