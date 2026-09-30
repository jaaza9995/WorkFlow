namespace WorkFlow.Api.Models;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}