using Clays.Application.Common.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Actions.UpdateActionStatus;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure() => Patch("/api/actions/{id:int}/status");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var action = await context.Actions.SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (action is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }
        action.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);
        var result = await context.Actions.AsNoTracking()
            .Include(item => item.Author)
            .Include(item => item.AssignedUser)
            .Include(item => item.Machine).ThenInclude(machine => machine!.Manufacturer)
            .SingleAsync(item => item.Id == request.Id, cancellationToken);
        await Send.OkAsync(result.Adapt<Response>(), cancellationToken);
    }
}
