using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.TimeEntries;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.Staff)]
public class TimeEntriesController : ControllerBase
{
    private readonly TimeEntryService _service;

    public TimeEntriesController(TimeEntryService service)
    {
        _service = service;
    }

    // GET: api/timeentries?projectId=1&taskId=4&userId=3
    [HttpGet]
    public async Task<ActionResult<List<TimeEntryDto>>> GetAll(
        [FromQuery] int? projectId,
        [FromQuery] int? taskId,
        [FromQuery] int? userId)
    {
        var entries = await _service.GetAllAsync(projectId, taskId, userId);
        return Ok(entries);
    }

    // GET: api/timeentries/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TimeEntryDto>> GetById(int id)
    {
        var entry = await _service.GetByIdAsync(id);
        if (entry is null) return NotFound();
        return Ok(entry);
    }

    // GET: api/timeentries/project/5/summary
    [Authorize(Roles = AppRoles.AdminOrManager)]
    [HttpGet("project/{projectId}/summary")]
    public async Task<ActionResult<ProjectHoursDto>> GetProjectHours(int projectId)
    {
        var summary = await _service.GetProjectHoursAsync(projectId);
        if (summary is null) return NotFound();
        return Ok(summary);
    }

    // POST: api/timeentries
    [HttpPost]
    public async Task<ActionResult<TimeEntryDto>> Create(CreateTimeEntryDto dto)
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

    // PUT: api/timeentries/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTimeEntryDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        if (!success) return NotFound();
        return NoContent();
    }

    // DELETE: api/timeentries/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}