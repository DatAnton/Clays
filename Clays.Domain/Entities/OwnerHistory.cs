namespace Clays.Domain.Entities;

public class OwnerHistory
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int MachineryId { get; set; }

    public Company Company { get; set; } = null!;
    public Machinery Machinery { get; set; } = null!;
}
