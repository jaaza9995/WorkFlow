using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Data;
using WorkFlow.Api.DTOs.Projects;
using WorkFlow.Api.DTOs.User;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

public class ProjectService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUser _user;

    public ProjectService(AppDbContext context, ICurrentUser user)
    {
        _context = context;
        _user = user;
    }

    // Felles mapping fra Project til ProjectDto. Brukes både for én og mange prosjekter,
    // og EF Core oversetter den til SQL (inkludert tellingen av oppgaver).
    private static readonly Expression<Func<Project, ProjectDto>> ToDto = p => new ProjectDto
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Status = p.Status,
        CompanyId = p.CompanyId,
        CustomerId = p.CustomerId,
        CustomerName = p.Customer.Name,
        ResponsibleManagerId = p.ResponsibleManagerId,
        ResponsibleManagerName = p.ResponsibleManager.FirstName + " " + p.ResponsibleManager.LastName,
        TotalTasks = p.Tasks.Count(),
        CompletedTasks = p.Tasks.Count(t => t.Status == WorkTaskStatus.Done)
    };

    public async Task<List<ProjectDto>> GetAllAsync(string? search, ProjectStatus? status, int? customerId)
    {
        var query = _context.Projects.VisibleTo(_user);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{search}%"));

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == customerId.Value);

        return await query.OrderBy(p => p.Name).Select(ToDto).ToListAsync();
    }

    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .VisibleTo(_user)
            .Where(p => p.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto)
    {
        await ValidateAsync(_user.CompanyId, dto.CustomerId, dto.ResponsibleManagerId, dto.StartDate, dto.EndDate);

        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status,
            CompanyId = _user.CompanyId,
            CustomerId = dto.CustomerId,
            ResponsibleManagerId = dto.ResponsibleManagerId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(project.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateProjectDto dto)
    {
        var project = await _context.Projects.VisibleTo(_user).FirstOrDefaultAsync(p => p.Id == id);
        if (project is null) return false;

        await ValidateAsync(project.CompanyId, dto.CustomerId, dto.ResponsibleManagerId, dto.StartDate, dto.EndDate);

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.Status = dto.Status;
        project.CustomerId = dto.CustomerId;
        project.ResponsibleManagerId = dto.ResponsibleManagerId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _context.Projects.VisibleTo(_user).FirstOrDefaultAsync(p => p.Id == id);
        if (project is null) return false;

        var hasRelatedData =
            await _context.Tasks.AnyAsync(t => t.ProjectId == id) ||
            await _context.Comments.AnyAsync(c => c.ProjectId == id);

        if (hasRelatedData)
            throw new InvalidOperationException("Prosjektet har oppgaver eller kommentarer og kan ikke slettes.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    // ---------- Ansatte på prosjektet (ProjectUsers) ----------

    public async Task<List<UserDto>?> GetUsersAsync(int projectId)
    {
        if (!await _context.Projects.VisibleTo(_user).AnyAsync(p => p.Id == projectId))
            return null;

        return await _context.ProjectUsers
            .Where(pu => pu.ProjectId == projectId)
            .Select(pu => new UserDto
            {
                Id = pu.User.Id,
                FirstName = pu.User.FirstName,
                LastName = pu.User.LastName,
                Email = pu.User.Email,
                Role = pu.User.Role,
                CompanyId = pu.User.CompanyId,
                CustomerId = pu.User.CustomerId
            })
            .ToListAsync();
    }

    public async Task<bool> AddUserAsync(int projectId, int userId)
    {
        var project = await _context.Projects.VisibleTo(_user).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project is null) return false;

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
            throw new ArgumentException("Brukeren finnes ikke.");

        if (user.CompanyId != project.CompanyId)
            throw new ArgumentException("Brukeren tilhører en annen bedrift enn prosjektet.");

        if (user.Role == UserRole.Customer)
            throw new ArgumentException("Kundebrukere får tilgang via kunden, ikke via prosjektets ansatte.");

        var alreadyMember = await _context.ProjectUsers
            .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);
        if (alreadyMember)
            throw new ArgumentException("Brukeren er allerede med i prosjektet.");

        _context.ProjectUsers.Add(new ProjectUser
        {
            ProjectId = projectId,
            UserId = userId,
            AssignedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveUserAsync(int projectId, int userId)
    {
        if (!await _context.Projects.VisibleTo(_user).AnyAsync(p => p.Id == projectId))
            return false;

        var link = await _context.ProjectUsers.FindAsync(projectId, userId);
        if (link is null) return false;

        var hasTasks = await _context.Tasks
            .AnyAsync(t => t.ProjectId == projectId && t.AssignedToUserId == userId);
        if (hasTasks)
            throw new InvalidOperationException("Brukeren har oppgaver i prosjektet. Tildel dem til andre først.");

        _context.ProjectUsers.Remove(link);
        await _context.SaveChangesAsync();
        return true;
    }

    // ---------- Felles validering ----------

    private async Task ValidateAsync(int companyId, int customerId, int managerId, DateOnly start, DateOnly? end)
    {
        if (end.HasValue && end.Value < start)
            throw new ArgumentException("Sluttdato kan ikke være før startdato.");

        if (!await _context.Companies.AnyAsync(c => c.Id == companyId))
            throw new ArgumentException("Bedriften finnes ikke.");

        var customer = await _context.Customers.FindAsync(customerId);
        if (customer is null)
            throw new ArgumentException("Kunden finnes ikke.");
        if (customer.CompanyId != companyId)
            throw new ArgumentException("Kunden tilhører en annen bedrift enn prosjektet.");

        var manager = await _context.Users.FindAsync(managerId);
        if (manager is null)
            throw new ArgumentException("Ansvarlig manager finnes ikke.");
        if (manager.Role != UserRole.Manager)
            throw new ArgumentException("Ansvarlig må være en bruker med rollen Manager.");
        if (manager.CompanyId != companyId)
            throw new ArgumentException("Ansvarlig manager tilhører en annen bedrift enn prosjektet.");
    }
}