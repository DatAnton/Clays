using Clays.Features.Companies.GetCompany;

namespace Clays.Features.Machinery.CreateMachinery;

public sealed record Response(
    int Id, int ManufacturerId, string ManufacturerName, string Model, int? Year, string? VinNumber,
    string? RegistrationNumber, string? CountryOfImport, int? PurchaseYear,
    Clays.Domain.Entities.Enums.MachineryStatus? Status, int CreatedById, int MachineryLibraryId);
