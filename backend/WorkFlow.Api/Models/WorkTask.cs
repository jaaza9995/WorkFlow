namespace WorkFlow.Api.Models;

public class WorkTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkTaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}