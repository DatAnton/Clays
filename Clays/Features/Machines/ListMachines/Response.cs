using Clays.Domain.Entities.Enums;

namespace Clays.Features.Machines.ListMachines;

public sealed record Response(IReadOnlyList<MachineItem> Items, int Page, int PageSize, int TotalCount);

public sealed record MachineItem(
    int Id,
    int ManufacturerId,
    string ManufacturerName,
    string Model,
    int? Year,
    string? VinNumber,
    string? RegistrationNumber,
    string? CountryOfImport,
    int? PurchaseYear,
    MachineStatus? Status,
    int? CompanyId,
    string? CompanyName,
    string? CompanyCity);
