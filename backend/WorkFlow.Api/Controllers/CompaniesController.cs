using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.Companies;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly CompanyService _service;

    public CompaniesController(CompanyService service)
    {
        _service = service;
    }

    // GET: api/companies
    [HttpGet]
    public async Task<ActionResult<List<CompanyDto>>> GetAllCompanies()
    {
        var companies = await _service.GetAllAsync();
        return Ok(companies);
    }

    // GET: api/companies/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetCompanyById(int id)
    {
        var company = await _service.GetByIdAsync(id);
        if (company is null) return NotFound();
        return Ok(company);
    }

    // POST: api/companies
    [HttpPost]
    public async Task<ActionResult<CompanyDto>> CreateCompany(CreateCompanyDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetCompanyById), new { id = created.Id }, created);
    }

    // PUT: api/companies/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, UpdateCompanyDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        if (!success) return NotFound();
        return NoContent();
    }

    // DELETE: api/companies/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}