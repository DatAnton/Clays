using Clays.Domain.Entities.Enums;

namespace Clays.Features.Machinery.ListMachinery;

public sealed record Response(IReadOnlyList<MachineryItem> Items, int Page, int PageSize, int TotalCount);

public sealed record MachineryItem(
    int Id,
    int ManufacturerId,
    string ManufacturerName,
    string Model,
    int? Year,
    string? VinNumber,
    string? RegistrationNumber,
    string? CountryOfImport,
    int? PurchaseYear,
    MachineryStatus? Status,
    int? CompanyId,
    string? CompanyName,
    string? CompanyCity,
    int MachineryLibraryId);
