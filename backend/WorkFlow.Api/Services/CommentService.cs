using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Data;
using WorkFlow.Api.DTOs.Comments;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

public class CommentService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUser _user;

    public CommentService(AppDbContext context, ICurrentUser user)
    {
        _context = context;
        _user = user;
    }

    private static readonly Expression<Func<Comment, CommentDto>> ToDto = c => new CommentDto
    {
        Id = c.Id,
        Content = c.Content,
        CreatedAt = c.CreatedAt,
        UserId = c.UserId,
        UserName = c.User.FirstName + " " + c.User.LastName,
        ProjectId = c.ProjectId,
        TaskId = c.TaskId
    };

    public async Task<List<CommentDto>> GetAllAsync(int? projectId, int? taskId)
    {
        var query = _context.Comments.VisibleTo(_user);

        if (projectId.HasValue)
            query = query.Where(c => c.ProjectId == projectId.Value);

        if (taskId.HasValue)
            query = query.Where(c => c.TaskId == taskId.Value);

        return await query.OrderBy(c => c.CreatedAt).Select(ToDto).ToListAsync();
    }

    public async Task<CommentDto?> GetByIdAsync(int id)
    {
        return await _context.Comments
            .VisibleTo(_user)
            .Where(c => c.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    public async Task<CommentDto> CreateAsync(CreateCommentDto dto)
    {
        // Prosjektet må være synlig for innlogget bruker (riktig bedrift, og for kunder: egen kunde)
        var project = await _context.Projects.VisibleTo(_user).FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
        if (project is null)
            throw new ArgumentException("Prosjektet finnes ikke.");

        if (dto.TaskId.HasValue)
        {
            var task = await _context.Tasks.FindAsync(dto.TaskId.Value);
            if (task is null)
                throw new ArgumentException("Oppgaven finnes ikke.");

            if (task.ProjectId != dto.ProjectId)
                throw new ArgumentException("Oppgaven tilhører et annet prosjekt.");
        }

        var comment = new Comment
        {
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UserId = _user.UserId,
            ProjectId = dto.ProjectId,
            TaskId = dto.TaskId
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(comment.Id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var comment = await _context.Comments.VisibleTo(_user).FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null) return false;

        var canDeleteOthers = _user.Role is UserRole.Administrator or UserRole.Manager;
        if (comment.UserId != _user.UserId && !canDeleteOthers)
            throw new UnauthorizedAccessException("Du kan bare slette dine egne kommentarer.");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }
}