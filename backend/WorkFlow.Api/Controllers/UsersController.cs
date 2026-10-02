using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.User;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UsersControllers : ControllerBase
{
    private readonly UserService _service;

    public UsersControllers(UserService service)
    {
        _service = service;
    }

    //GET: api/user
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var user = await _service.GetAllAsync();
        return Ok(user);
    }
}