using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectUser> ProjectUsers => Set<ProjectUser>();
    public DbSet<WorkTask> Tasks => Set<WorkTask>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enums lagres som tekst, ikke som tall
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<Project>()
            .Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<WorkTask>()
            .Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<WorkTask>()
            .Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Unik e-post
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Sammensatt PK for koblingstabellen ProjectUsers
        modelBuilder.Entity<ProjectUser>()
            .HasKey(pu => new { pu.ProjectId, pu.UserId });

        // ProjectUsers: mange-til-mange mellom Users og Projects
        modelBuilder.Entity<ProjectUser>()
            .HasOne(pu => pu.Project)
            .WithMany(p => p.ProjectUsers)
            .HasForeignKey(pu => pu.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectUser>()
            .HasOne(pu => pu.User)
            .WithMany(u => u.ProjectUsers)
            .HasForeignKey(pu => pu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Project -> ResponsibleManager (separat fra ProjectUsers)
        modelBuilder.Entity<Project>()
            .HasOne(p => p.ResponsibleManager)
            .WithMany(u => u.ManagedProjects)
            .HasForeignKey(p => p.ResponsibleManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Task -> AssignedToUser (nullable)
        modelBuilder.Entity<WorkTask>()
            .HasOne(t => t.AssignedToUser)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Comment -> Task (nullable, prosjektkommentar vs. oppgavekommentar)
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // Check constraints
        modelBuilder.Entity<Project>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Project_EndDate",
                "\"EndDate\" IS NULL OR \"EndDate\" >= \"StartDate\""));

        modelBuilder.Entity<TimeEntry>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_TimeEntry_Hours",
                "\"Hours\" > 0 AND \"Hours\" <= 24"));

        // Indekser for filtrering
        modelBuilder.Entity<WorkTask>().HasIndex(t => t.Status);
        modelBuilder.Entity<WorkTask>().HasIndex(t => t.DueDate);
    }
}