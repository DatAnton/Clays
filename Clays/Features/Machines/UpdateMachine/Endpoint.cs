using Clays.Application.Common.Interfaces;
using Clays.Features.Machines.CreateMachine;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Machines.UpdateMachine;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure() => Put("/api/machines/{id:int}");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var machine = await context.Machinery.SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (machine is null)
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

        machine.MachineryLibraryId = libraryEntry.Id;
        machine.Year = request.Year;
        machine.VinNumber = request.VinNumber?.Trim();
        machine.RegistrationNumber = request.RegistrationNumber?.Trim();
        machine.CountryOfImport = request.CountryOfImport?.Trim();
        machine.PurchaseYear = request.PurchaseYear;
        machine.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);

        await Send.OkAsync(new Response(
            machine.Id, libraryEntry.ManufacturerId, libraryEntry.Manufacturer.Name, libraryEntry.Model, machine.Year,
            machine.VinNumber, machine.RegistrationNumber, machine.CountryOfImport,
            machine.PurchaseYear, machine.Status, machine.CreatedById, machine.MachineryLibraryId), cancellationToken);
    }
}
