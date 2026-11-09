using Clays.Application.Common.Interfaces;
using Clays.Features.Machines.CreateMachine;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Machines.UpdateMachine;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure() => Put("/api/machines/{id:int}");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var machine = await context.Machines.SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (machine is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        var manufacturer = await context.Manufacturers.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.ManufacturerId, cancellationToken);
        if (manufacturer is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        machine.ManufacturerId = request.ManufacturerId;
        machine.Model = request.Model.Trim();
        machine.Year = request.Year;
        machine.VinNumber = request.VinNumber?.Trim();
        machine.RegistrationNumber = request.RegistrationNumber?.Trim();
        machine.CountryOfImport = request.CountryOfImport?.Trim();
        machine.PurchaseYear = request.PurchaseYear;
        machine.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);

        await Send.OkAsync(new Response(
            machine.Id, machine.ManufacturerId, manufacturer.Name, machine.Model, machine.Year,
            machine.VinNumber, machine.RegistrationNumber, machine.CountryOfImport,
            machine.PurchaseYear, machine.Status, machine.CreatedById), cancellationToken);
    }
}
