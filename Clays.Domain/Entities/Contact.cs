namespace Clays.Domain.Entities;

public class Contact
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}