using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Clays.Features.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Actions.CreateAction;

public sealed class Endpoint(IApplicationDbContext context, ICurrentUserService currentUser) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure() => Post("/api/actions");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        if (!await context.Companies.AnyAsync(company => company.Id == request.CompanyId, cancellationToken))
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }
        User? assignedUser = null;
        if (request.AssignedUserId.HasValue)
        {
            assignedUser = await context.Users
                .SingleOrDefaultAsync(user => user.Id == request.AssignedUserId.Value, cancellationToken);
            if (assignedUser is null)
            {
                await Send.NotFoundAsync(cancellationToken);
                return;
            }
        }
        if (request.MachineId.HasValue && !await context.Machines.AnyAsync(machine => machine.Id == request.MachineId, cancellationToken))
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        var action = request.Adapt<Domain.Entities.Action>();
        action.AuthorId = currentUser.UserDatabaseId;
        action.AssignedUserId = assignedUser?.Id;
        action.CreatedDate = DateTime.UtcNow;
        context.Actions.Add(action);
        await context.SaveChangesAsync(cancellationToken);

        var savedAction = await context.Actions.AsNoTracking()
            .Include(item => item.Author)
            .Include(item => item.AssignedUser)
            .Include(item => item.Machine).ThenInclude(machine => machine!.Manufacturer)
            .SingleAsync(item => item.Id == action.Id, cancellationToken);
        await EndpointResponse.JsonAsync(HttpContext, savedAction.Adapt<Response>(), 201, cancellationToken);
    }
}
