using Clays.Domain.Entities.Enums;

namespace Clays.Domain.Entities
{
    public class Machine
    {
        public int Id { get; set; }
        public int ManufacturerId { get; set; }
        public string Model { get; set; } = string.Empty;
        public int? Year { get; set; }
        public string? VinNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? CountryOfImport { get; set; }
        public int? PurchaseYear { get; set; }
        public MachineStatus? Status { get; set; }
        public int CreatedById { get; set; }

        public Manufacturer Manufacturer { get; set; } = null!;
        public User? CreatedBy { get; set; }
    }
}
