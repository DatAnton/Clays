namespace Clays.Features.Contacts.UpdateContact;

public sealed class Request
{
    public int Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public int CompanyId { get; init; }
    public int? MachineryId { get; init; }
    public string? Comment { get; init; }
}
