using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.Dashboard;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(DashboardService service)
    {
        _service = service;
    }

    // GET: api/dashboard
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        return Ok(await _service.GetAsync());
    }
}