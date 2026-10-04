namespace WorkFlow.Api.DTOs.Customers;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int CompanyId { get; set; }
}

public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // Midlertidig: sendes eksplisitt til vi har innlogging i Fase 4
    public int CompanyId { get; set; }
}

public class UpdateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}