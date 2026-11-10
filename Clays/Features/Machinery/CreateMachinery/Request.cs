using Clays.Domain.Entities.Enums;

namespace Clays.Features.Machinery.CreateMachinery;

public sealed class Request
{
    public int MachineryLibraryId { get; init; }
    public int? Year { get; init; }
    public string? VinNumber { get; init; }
    public string? RegistrationNumber { get; init; }
    public string? CountryOfImport { get; init; }
    public int? PurchaseYear { get; init; }
    public MachineryStatus? Status { get; init; }
    public int? CompanyId { get; init; }
}
