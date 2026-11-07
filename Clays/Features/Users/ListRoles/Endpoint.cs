using Clays.Application.Common.Interfaces;
using Clays.Services;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Users.ListRoles;

public sealed class Endpoint(IApplicationDbContext context)
    : FastEndpoints.EndpointWithoutRequest<IReadOnlyList<RoleItem>>
{
    public override void Configure()
    {
        Get("/api/users/roles");
        Policies(AdminAuthorization.Policy);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var roles = await context.Roles.AsNoTracking().OrderBy(role => role.Id)
            .Select(role => new RoleItem(role.Id, role.Name)).ToListAsync(ct);

        await Send.OkAsync(roles, ct);
    }
}
