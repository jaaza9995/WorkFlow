using System.ComponentModel.DataAnnotations;

namespace WorkFlow.Api.DTOs.Comments;

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
}

public class CreateCommentDto
{
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }

    // Tom = prosjektkommentar, satt = oppgavekommentar
    public int? TaskId { get; set; }
}