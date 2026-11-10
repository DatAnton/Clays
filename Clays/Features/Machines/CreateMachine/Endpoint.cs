using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Clays.Features.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Machines.CreateMachine;

public sealed class Endpoint(IApplicationDbContext context, ICurrentUserService currentUser) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure() => Post("/api/machines");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var libraryEntry = await context.MachineryLibrary
            .Include(item => item.Manufacturer)
            .SingleOrDefaultAsync(item => item.Id == request.MachineryLibraryId, cancellationToken);
        if (libraryEntry is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }
        if (request.CompanyId.HasValue && !await context.Companies.AnyAsync(company => company.Id == request.CompanyId, cancellationToken))
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }
        var machine = request.Adapt<Machine>();
        machine.MachineryLibraryId = libraryEntry.Id;
        machine.CreatedById = currentUser.UserDatabaseId;
        context.Machinery.Add(machine);
        await context.SaveChangesAsync(cancellationToken);

        await EndpointResponse.JsonAsync(HttpContext, new Response(
            machine.Id, libraryEntry.ManufacturerId, libraryEntry.Manufacturer.Name, libraryEntry.Model, machine.Year,
            machine.VinNumber, machine.RegistrationNumber, machine.CountryOfImport,
            machine.PurchaseYear, machine.Status, machine.CreatedById, machine.MachineryLibraryId), 201, cancellationToken);
    }
}
