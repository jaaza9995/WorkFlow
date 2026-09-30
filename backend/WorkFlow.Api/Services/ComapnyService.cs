using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Data;
using WorkFlow.Api.DTOs.Companies;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

public class CompanyService
{
    private readonly AppDbContext _context;

    public CompanyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyDto>> GetAllAsync()
    {
        return await _context.Companies
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company is null) return null;

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            CreatedAt = company.CreatedAt
        };
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyDto dto)
    {
        var company = new Company
        {
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            CreatedAt = company.CreatedAt
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateCompanyDto dto)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company is null) return false;

        company.Name = dto.Name;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company is null) return false;

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();
        return true;
    }
}