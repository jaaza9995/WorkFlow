using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.Projects;
using WorkFlow.Api.DTOs.User;
using WorkFlow.Api.Models;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _service;

    public ProjectsController(ProjectService service)
    {
        _service = service;
    }

    // GET: api/projects?search=web&status=Active&customerId=2
    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] ProjectStatus? status,
        [FromQuery] int? customerId)
    {
        var projects = await _service.GetAllAsync(search, status, customerId);
        return Ok(projects);
    }

    // GET: api/projects/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetById(int id)
    {
        var project = await _service.GetByIdAsync(id);
        if (project is null) return NotFound();
        return Ok(project);
    }

    // POST: api/projects
    [Authorize(Roles = AppRoles.AdminOrManager)]
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectDto dto)
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

    // PUT: api/projects/5
    [Authorize(Roles = AppRoles.AdminOrManager)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProjectDto dto)
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

    // DELETE: api/projects/5
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

    // GET: api/projects/5/users
    [Authorize(Roles = AppRoles.Staff)]
    [HttpGet("{id}/users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers(int id)
    {
        var users = await _service.GetUsersAsync(id);
        if (users is null) return NotFound();
        return Ok(users);
    }

    // POST: api/projects/5/users/12
    [Authorize(Roles = AppRoles.AdminOrManager)]
    [HttpPost("{id}/users/{userId}")]
    public async Task<IActionResult> AddUser(int id, int userId)
    {
        try
        {
            var success = await _service.AddUserAsync(id, userId);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/projects/5/users/12
    [Authorize(Roles = AppRoles.AdminOrManager)]
    [HttpDelete("{id}/users/{userId}")]
    public async Task<IActionResult> RemoveUser(int id, int userId)
    {
        try
        {
            var success = await _service.RemoveUserAsync(id, userId);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}