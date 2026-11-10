using Clays.Domain.Entities.Enums;

namespace Clays.Domain.Entities
{
    public class Machine
    {
        public int Id { get; set; }
        public int MachineryLibraryId { get; set; }
        public int? Year { get; set; }
        public string? VinNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? CountryOfImport { get; set; }
        public int? PurchaseYear { get; set; }
        public MachineStatus? Status { get; set; }
        public int CreatedById { get; set; }
        public int? CompanyId { get; set; }

        public MachineryLibrary MachineryLibrary { get; set; } = null!;
        public User? CreatedBy { get; set; }
        public Company? Company { get; set; }
    }
}
