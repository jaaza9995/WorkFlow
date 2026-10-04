namespace WorkFlow.Api.DTOs.Companies;

// Det API-et returnerer når man henter en eller flere bedrifter
public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Det API-et forventer å motta når man oppretter en ny bedrift
public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;
}

// Det API-et forventer å motta når man endrer en eksisterende bedrift
public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;
}