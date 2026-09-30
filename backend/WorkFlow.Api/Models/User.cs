namespace WorkFlow.Api.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    // Kun satt når Role == UserRole.Customer
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();

    // Prosjekter der denne brukeren er ansvarlig manager
    public ICollection<Project> ManagedProjects { get; set; } = new List<Project>();

    // Oppgaver denne brukeren er tildelt
    public ICollection<WorkTask> AssignedTasks { get; set; } = new List<WorkTask>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}