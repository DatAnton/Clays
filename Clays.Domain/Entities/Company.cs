
using Clays.Domain.Entities.Enums;

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
    public CompanyStatus? Status { get; set; }
    public string? Note { get; set; }
    public int? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<CompanyComment> Comments { get; set; } = new List<CompanyComment>();
    public ICollection<OwnerHistory> OwnerHistory { get; set; } = new List<OwnerHistory>();
    public ICollection<Action> Actions { get; set; } = new List<Action>();
}