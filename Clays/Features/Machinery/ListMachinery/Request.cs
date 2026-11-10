using Clays.Domain.Entities.Enums;

namespace Clays.Features.Machinery.ListMachinery;

public sealed class Request
{
    public int Page { get; set; } = 1;
    public string? Search { get; set; }
    public int? ManufacturerId { get; set; }
    public int? MachineryLibraryId { get; set; }
    public MachineryStatus? Status { get; set; }
}
