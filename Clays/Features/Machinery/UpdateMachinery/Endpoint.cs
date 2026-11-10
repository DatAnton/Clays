using Clays.Application.Common.Interfaces;
using Clays.Features.Machinery.CreateMachinery;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Machinery.UpdateMachinery;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure() => Put("/api/machinery/{id:int}");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var machinery = await context.Machinery.SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (machinery is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        var libraryEntry = await context.MachineryLibrary.AsNoTracking()
            .Include(item => item.Manufacturer)
            .SingleOrDefaultAsync(item => item.Id == request.MachineryLibraryId, cancellationToken);
        if (libraryEntry is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        machinery.MachineryLibraryId = libraryEntry.Id;
        machinery.Year = request.Year;
        machinery.VinNumber = request.VinNumber?.Trim();
        machinery.RegistrationNumber = request.RegistrationNumber?.Trim();
        machinery.CountryOfImport = request.CountryOfImport?.Trim();
        machinery.PurchaseYear = request.PurchaseYear;
        machinery.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);

        await Send.OkAsync(new Response(
            machinery.Id, libraryEntry.ManufacturerId, libraryEntry.Manufacturer.Name, libraryEntry.Model, machinery.Year,
            machinery.VinNumber, machinery.RegistrationNumber, machinery.CountryOfImport,
            machinery.PurchaseYear, machinery.Status, machinery.CreatedById, machinery.MachineryLibraryId), cancellationToken);
    }
}
