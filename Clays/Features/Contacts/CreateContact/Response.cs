namespace Clays.Features.Contacts.CreateContact;

public sealed record Response(int Id, string? FirstName, string? LastName, string? PhoneNumber, string? Email, int CompanyId);
