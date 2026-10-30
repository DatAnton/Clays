namespace Clays.Features.Companies.ListCompanies;

public sealed class Request
{
    public int Page { get; set; } = 1;
    public string? Search { get; set; }
    public string? City { get; set; }
    public string? BusinessLine { get; set; }
    public bool? IsOurPartner { get; set; }
}
