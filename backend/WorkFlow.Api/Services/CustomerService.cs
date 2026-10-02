using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Api.Data;
using WorkFlow.Api.DTOs.Customer;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

public class CustomerService
{
    private readonly AppContext _context;

    public CustomerService(AppContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        return await _context.Customer
        .Select(c => new CustomerDto
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone
        })
        .ToListAsync();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _context.Customer.FindAsync(id);
        if (customer is null) return null;

        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone
        };
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
        };
        
        _context.Customer.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
        };
    }
    public async Task<bool> UpdateAsync(int id, UpdateCustomer dto)
    {
        var customer = await _context.Customer.FindAsync(id);
        if (customer is null) return false;

        customer.FirstName = dto.Name;
        customer.LastName = dto.LastName;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _context.Customer.FindAsync(id);
        if (customer is null) return false;

        _context.Customer.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }

}