using Clays.Domain.Entities.Enums;

namespace Clays.Features.Machines.ListMachines;

public sealed class Request
{
    public int Page { get; set; } = 1;
    public string? Search { get; set; }
    public int? ManufacturerId { get; set; }
    public MachineStatus? Status { get; set; }
    public string? City { get; set; }
}
