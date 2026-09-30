namespace WorkFlow.Api.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public ProjectStatus Status { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int ResponsibleManagerId { get; set; }
    public User ResponsibleManager { get; set; } = null!;

    public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}