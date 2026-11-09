namespace Clays.Features.Companies.CreateCompany;

public sealed class Request
{
    public string Name { get; init; } = string.Empty;
    public string? BusinessId { get; init; }
    public string? City { get; init; }
    public string? Address { get; init; }
    public string? MainBusinessLine { get; init; }
    public bool IsOurPartner { get; init; }
}
