using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.Comments;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly CommentService _service;

    public CommentsController(CommentService service)
    {
        _service = service;
    }

    // GET: api/comments?projectId=1&taskId=4
    [HttpGet]
    public async Task<ActionResult<List<CommentDto>>> GetAll(
        [FromQuery] int? projectId,
        [FromQuery] int? taskId)
    {
        var comments = await _service.GetAllAsync(projectId, taskId);
        return Ok(comments);
    }

    // GET: api/comments/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetById(int id)
    {
        var comment = await _service.GetByIdAsync(id);
        if (comment is null) return NotFound();
        return Ok(comment);
    }

    // POST: api/comments
    [HttpPost]
    public async Task<ActionResult<CommentDto>> Create(CreateCommentDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/comments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}