namespace WorkFlow.Api.Models;

public class TimeEntry
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TaskId { get; set; }
    public WorkTask Task { get; set; } = null!;
}