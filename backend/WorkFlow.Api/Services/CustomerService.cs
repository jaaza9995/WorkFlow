using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Data;
using WorkFlow.Api.DTOs.Customers;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

public class CustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        return await _context.Customers
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                CompanyId = c.CompanyId
            })
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null) return null;

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            CompanyId = customer.CompanyId
        };
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            CompanyId = dto.CompanyId
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            CompanyId = customer.CompanyId
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null) return false;

        customer.Name = dto.Name;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null) return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }
}