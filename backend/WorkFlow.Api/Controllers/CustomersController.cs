using System.Runtime.Versioning;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.Customer;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;

[ApiController]
[ResourceConsumption("api/[Controller]")]

public class CustomersController : ControllerBase
{
    private readonly Customer _service;


    public CustomersController(CustomerService service)
    {
        _service = service;
    }

    //GET: api/customers
    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAllCustomers()
    {
        var customers = await _service.GetAllAsync();
        return Ok(customers);
    }

    //GET: api/Customers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
    {
        var customer = await _service.GetByIdAsync(id);
        if (customer is null) return NotFound();
        return Ok(customer);
    }


    //POST: api/customer
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAction(nameof(GetCustomerById), new { id = created.Id}, created);
    }


    //PUT: api/customers/5

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomers(int id, UpdateCustomerDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        if(!success) return NotFound();
        return NoContent();

    }
}