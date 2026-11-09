using Clays.Domain.Entities.Enums;

namespace Clays.Features.Companies.UpdateCompany;

public sealed class Request
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? BusinessId { get; init; }
    public string? City { get; init; }
    public string? Address { get; init; }
    public string? MainBusinessLine { get; init; }
    public bool IsOurPartner { get; init; }
    public CompanyStatus? Status { get; init; }
    public string? Note { get; init; }
}
