using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

// Begrenser spørringer til det innlogget bruker har lov til å se.
// Her ligger både dataisolasjon mellom bedrifter og rollenes synlighet:
//   Administrator/Manager: alt i egen bedrift
//   Employee:              prosjekter de er med i (og oppgavene/kommentarene der)
//   Customer:              prosjekter som tilhører egen kunde
public static class QueryScopes
{
    public static IQueryable<Company> VisibleTo(this IQueryable<Company> query, ICurrentUser user)
    {
        var companyId = user.CompanyId;
        return query.Where(c => c.Id == companyId);
    }

    public static IQueryable<Customer> VisibleTo(this IQueryable<Customer> query, ICurrentUser user)
    {
        var companyId = user.CompanyId;
        return query.Where(c => c.CompanyId == companyId);
    }

    public static IQueryable<User> VisibleTo(this IQueryable<User> query, ICurrentUser user)
    {
        var companyId = user.CompanyId;
        return query.Where(u => u.CompanyId == companyId);
    }

    public static IQueryable<Project> VisibleTo(this IQueryable<Project> query, ICurrentUser user)
    {
        var companyId = user.CompanyId;
        var userId = user.UserId;
        var customerId = user.CustomerId;

        return user.Role switch
        {
            UserRole.Customer => query.Where(p => p.CompanyId == companyId && p.CustomerId == customerId),
            UserRole.Employee => query.Where(p => p.CompanyId == companyId && p.ProjectUsers.Any(pu => pu.UserId == userId)),
            _ => query.Where(p => p.CompanyId == companyId)
        };
    }

    public static IQueryable<WorkTask> VisibleTo(this IQueryable<WorkTask> query, ICurrentUser user)
    {
        var companyId = user.CompanyId;
        var userId = user.UserId;
        var customerId = user.CustomerId;

        return user.Role switch
        {
            UserRole.Customer => query.Where(t => t.Project.CompanyId == companyId && t.Project.CustomerId == customerId),
            UserRole.Employee => query.Where(t => t.Project.CompanyId == companyId && t.Project.ProjectUsers.Any(pu => pu.UserId == userId)),
            _ => query.Where(t => t.Project.CompanyId == companyId)
        };
    }

    public static IQueryable<Comment> VisibleTo(this IQueryable<Comment> query, ICurrentUser user)
    {
        var companyId = user.CompanyId;
        var userId = user.UserId;
        var customerId = user.CustomerId;

        return user.Role switch
        {
            UserRole.Customer => query.Where(c => c.Project.CompanyId == companyId && c.Project.CustomerId == customerId),
            UserRole.Employee => query.Where(c => c.Project.CompanyId == companyId && c.Project.ProjectUsers.Any(pu => pu.UserId == userId)),
            _ => query.Where(c => c.Project.CompanyId == companyId)
        };
    }

    // Timer: ansatte ser egne, managere/administratorer ser hele bedriften, kunder ser ingen
    public static IQueryable<TimeEntry> VisibleTo(this IQueryable<TimeEntry> query, ICurrentUser user)
    {
        var companyId = user.CompanyId;
        var userId = user.UserId;

        return user.Role switch
        {
            UserRole.Customer => query.Where(te => false),
            UserRole.Employee => query.Where(te => te.UserId == userId),
            _ => query.Where(te => te.Task.Project.CompanyId == companyId)
        };
    }
}