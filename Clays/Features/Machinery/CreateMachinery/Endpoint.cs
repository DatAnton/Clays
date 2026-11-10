using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using MachineryEntity = Clays.Domain.Entities.Machinery;
using Clays.Features.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Machinery.CreateMachinery;

public sealed class Endpoint(IApplicationDbContext context, ICurrentUserService currentUser) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure() => Post("/api/machinery");

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
        var machinery = request.Adapt<MachineryEntity>();
        machinery.MachineryLibraryId = libraryEntry.Id;
        machinery.CreatedById = currentUser.UserDatabaseId;
        context.Machinery.Add(machinery);
        await context.SaveChangesAsync(cancellationToken);

        await EndpointResponse.JsonAsync(HttpContext, new Response(
            machinery.Id, libraryEntry.ManufacturerId, libraryEntry.Manufacturer.Name, libraryEntry.Model, machinery.Year,
            machinery.VinNumber, machinery.RegistrationNumber, machinery.CountryOfImport,
            machinery.PurchaseYear, machinery.Status, machinery.CreatedById, machinery.MachineryLibraryId), 201, cancellationToken);
    }
}
