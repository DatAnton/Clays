namespace Clays.Features.MachineryLibrary.UpdateMachineryLibrary;

public sealed class Request
{
    public int Id { get; init; }
    public int ManufacturerId { get; init; }
    public string Model { get; init; } = string.Empty;
}
