using Clays.Domain.Entities.Enums;

namespace Clays.Features.Machines.UpdateMachine;

public sealed class Request
{
    public int Id { get; init; }
    public int MachineryLibraryId { get; init; }
    public int? Year { get; init; }
    public string? VinNumber { get; init; }
    public string? RegistrationNumber { get; init; }
    public string? CountryOfImport { get; init; }
    public int? PurchaseYear { get; init; }
    public MachineStatus? Status { get; init; }
}
