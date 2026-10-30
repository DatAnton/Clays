namespace Clays.Features.Companies.ListCompanies;

public sealed record Response(IReadOnlyList<CompanyItem> Items, int Page, int PageSize, int TotalCount);

public sealed record CompanyItem(
    int Id,
    string Name,
    string? BusinessId,
    string? City,
    string? Address,
    string? MainBusinessLine,
    bool IsOurPartner);
