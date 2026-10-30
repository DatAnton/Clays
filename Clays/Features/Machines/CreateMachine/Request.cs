using Clays.Domain.Entities.Enums;

namespace Clays.Features.Machines.CreateMachine;

public sealed class Request
{
    public int ManufacturerId { get; init; }
    public string Model { get; init; } = string.Empty;
    public int? Year { get; init; }
    public string? VinNumber { get; init; }
    public string? RegistrationNumber { get; init; }
    public string? CountryOfImport { get; init; }
    public int? PurchaseYear { get; init; }
    public MachineStatus? Status { get; init; }
    public int? CompanyId { get; init; }
}
