using Clays.Application.Common.Interfaces;
using Clays.Features.Common;
using Clays.Features.MachineryLibrary.ListMachineryLibrary;
using Clays.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Clays.Features.MachineryLibrary.UpdateMachineryLibrary;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, LibraryItem>
{
    public override void Configure()
    {
        Put("/api/machinery-library/{id:int}");
        Policies(AdminAuthorization.Policy);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var entry = await context.MachineryLibrary.SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (entry is null)
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

        var model = request.Model.Trim();
        if (await context.MachineryLibrary.AnyAsync(item => item.ManufacturerId == manufacturer.Id
            && item.Model == model && item.Id != request.Id, cancellationToken))
        {
            await SendDuplicateAsync(cancellationToken);
            return;
        }

        entry.ManufacturerId = manufacturer.Id;
        entry.Model = model;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_MachineryLibrary_ManufacturerId_Model" })
        {
            await SendDuplicateAsync(cancellationToken);
            return;
        }

        await EndpointResponse.JsonAsync(HttpContext,
            new LibraryItem(entry.Id, manufacturer.Id, manufacturer.Name, entry.Model), 200, cancellationToken);
    }

    private Task SendDuplicateAsync(CancellationToken cancellationToken) =>
        EndpointResponse.JsonAsync(HttpContext, new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "This manufacturer and model already exist in the machinery library."
        }, StatusCodes.Status409Conflict, cancellationToken);
}
