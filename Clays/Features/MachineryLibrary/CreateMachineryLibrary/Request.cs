namespace Clays.Features.MachineryLibrary.CreateMachineryLibrary;

public sealed class Request
{
    public int ManufacturerId { get; init; }
    public string Model { get; init; } = string.Empty;
}
