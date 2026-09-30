namespace WorkFlow.Api.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Null = prosjektkommentar, satt = oppgavekommentar
    public int? TaskId { get; set; }
    public WorkTask? Task { get; set; }
}