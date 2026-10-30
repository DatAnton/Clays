using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Clays.Features.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Machines.CreateMachine;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure() => Post("/api/machines");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var manufacturer = await context.Manufacturers
            .SingleOrDefaultAsync(item => item.Id == request.ManufacturerId, cancellationToken);
        if (manufacturer is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }
        if (request.CompanyId.HasValue && !await context.Companies.AnyAsync(company => company.Id == request.CompanyId, cancellationToken))
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }
        var actor = await context.Users.FirstOrDefaultAsync(cancellationToken);
        if (actor is null)
        {
            await Send.UnauthorizedAsync(cancellationToken);
            return;
        }

        var machine = request.Adapt<Machine>();
        machine.Model = request.Model.Trim();
        machine.CreatedById = actor.Id;
        context.Machines.Add(machine);
        await context.SaveChangesAsync(cancellationToken);

        await EndpointResponse.JsonAsync(HttpContext, new Response(
            machine.Id, machine.ManufacturerId, manufacturer.Name, machine.Model, machine.Year,
            machine.VinNumber, machine.RegistrationNumber, machine.CountryOfImport,
            machine.PurchaseYear, machine.Status, machine.CreatedById), 201, cancellationToken);
    }
}
