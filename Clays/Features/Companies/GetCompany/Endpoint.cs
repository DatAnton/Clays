using Clays.Application.Common.Interfaces;
using FastEndpoints;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Companies.GetCompany;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.EndpointWithoutRequest<Response>
{
    public override void Configure() => Get("/api/companies/{id:int}");

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var companyId = Route<int>("id");
        var company = await context.Companies.AsNoTracking().AsSplitQuery()
            .Include(item => item.CreatedBy)
            .Include(item => item.Contacts)
            .Include(item => item.Comments).ThenInclude(comment => comment.Author)
            .Include(item => item.Machines).ThenInclude(machine => machine.Manufacturer)
            .Include(item => item.OwnerHistory).ThenInclude(history => history.Machine).ThenInclude(machine => machine.Manufacturer)
            .Include(item => item.Actions).ThenInclude(action => action.Author)
            .Include(item => item.Actions).ThenInclude(action => action.AssignedUser)
            .Include(item => item.Actions).ThenInclude(action => action.Machine).ThenInclude(machine => machine!.Manufacturer)
            .SingleOrDefaultAsync(item => item.Id == companyId, cancellationToken);

        if (company is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        await Send.OkAsync(company.Adapt<Response>(), cancellationToken);
    }
}
