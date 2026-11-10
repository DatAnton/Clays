using Clays.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.MachineryLibrary.ListMachineryLibrary;

public sealed record LibraryItem(int Id, int ManufacturerId, string ManufacturerName, string Model);

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.EndpointWithoutRequest<List<LibraryItem>>
{
    public override void Configure() => Get("/api/machinery-library");

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var entries = await context.MachineryLibrary.AsNoTracking()
            .OrderBy(entry => entry.Manufacturer.Name).ThenBy(entry => entry.Model).ThenBy(entry => entry.Id)
            .Select(entry => new LibraryItem(entry.Id, entry.ManufacturerId, entry.Manufacturer.Name, entry.Model))
            .ToListAsync(cancellationToken);
        await Send.OkAsync(entries, cancellationToken);
    }
}
