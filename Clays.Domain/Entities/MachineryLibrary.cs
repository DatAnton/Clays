namespace Clays.Domain.Entities;

public class MachineryLibrary
{
    public int Id { get; set; }
    public int ManufacturerId { get; set; }
    public string Model { get; set; } = string.Empty;

    public Manufacturer Manufacturer { get; set; } = null!;
}
