namespace Clays.Domain.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Address { get; set; }
    public bool IsOurPartner { get; set; } = false;
    public string? BusinessId { get; set; }
    public string? MainBusinessLine { get; set; }
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}