using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.Customers;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _service;

    public CustomersController(CustomerService service)
    {
        _service = service;
    }

    // GET: api/customers
    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll()
    {
        var customers = await _service.GetAllAsync();
        return Ok(customers);
    }

    // GET: api/customers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _service.GetByIdAsync(id);
        if (customer is null) return NotFound();
        return Ok(customer);
    }

    // POST: api/customers
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/customers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCustomerDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        if (!success) return NotFound();
        return NoContent();
    }

    // DELETE: api/customers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}