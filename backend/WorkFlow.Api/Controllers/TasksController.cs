using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.Tasks;
using WorkFlow.Api.Models;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskService _service;

    public TasksController(TaskService service)
    {
        _service = service;
    }

    // GET: api/tasks?projectId=1&status=InProgress&assignedToUserId=3&search=design
    [HttpGet]
    public async Task<ActionResult<List<TaskDto>>> GetAll(
        [FromQuery] int? projectId,
        [FromQuery] WorkTaskStatus? status,
        [FromQuery] int? assignedToUserId,
        [FromQuery] string? search)
    {
        var tasks = await _service.GetAllAsync(projectId, status, assignedToUserId, search);
        return Ok(tasks);
    }

    // GET: api/tasks/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetById(int id)
    {
        var task = await _service.GetByIdAsync(id);
        if (task is null) return NotFound();
        return Ok(task);
    }

    // POST: api/tasks
    [Authorize(Roles = AppRoles.AdminOrManager)]
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
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

    // PUT: api/tasks/5
    [Authorize(Roles = AppRoles.AdminOrManager)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTaskDto dto)
    {
        try
        {
            var success = await _service.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/tasks/5/status
    [Authorize(Roles = AppRoles.Staff)]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateTaskStatusDto dto)
    {
        var success = await _service.UpdateStatusAsync(id, dto.Status);
        if (!success) return NotFound();
        return NoContent();
    }

    // DELETE: api/tasks/5
    [Authorize(Roles = AppRoles.AdminOrManager)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}