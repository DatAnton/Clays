namespace Clays.Domain.Entities;

public class OwnerHistory
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int MachineId { get; set; }

    public Company Company { get; set; } = null!;
    public Machine Machine { get; set; } = null!;
}
