using System.Runtime.Versioning;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.Customer;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;

[ApiController]
[ResourceConsumption("api/[Controller]")]

public class CustomerController : ControllerBase
{
    private readonly Customer _service;
    
}