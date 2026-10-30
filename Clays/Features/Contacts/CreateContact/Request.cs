namespace Clays.Features.Contacts.CreateContact;

public sealed class Request
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public int CompanyId { get; init; }
}
