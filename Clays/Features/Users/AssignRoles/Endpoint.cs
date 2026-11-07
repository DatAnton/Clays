using Clays.Application.Common.Interfaces;
using Clays.Domain.Constants;
using Clays.Domain.Entities;
using Clays.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Clays.Features.Users.AssignRoles;

public sealed class Endpoint(IApplicationDbContext context, ICurrentUserService currentUser)
    : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure()
    {
        Put("/api/users/{Id:int}/roles");
        Policies(AdminAuthorization.Policy);
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        var user = await context.Users.Include(user => user.UserRoles)
            .SingleOrDefaultAsync(user => user.Id == request.Id, ct);
        if (user is null) { await Send.NotFoundAsync(ct); return; }

        var desiredRoleIds = request.RoleIds.ToHashSet();
        var roles = await context.Roles.AsNoTracking().Where(role => desiredRoleIds.Contains(role.Id))
            .OrderBy(role => role.Id).Select(role => new RoleItem(role.Id, role.Name)).ToListAsync(ct);
        if (roles.Count != desiredRoleIds.Count)
            throw new UserManagementException(400, "One or more selected roles do not exist.");

        var currentUserRoleIds = user.UserRoles.Select(userRole => userRole.RoleId).ToHashSet();
        if (!currentUserRoleIds.SetEquals(request.ExpectedRoleIds))
            throw new UserManagementException(409, "This user's roles changed. Refresh the page before editing them again.");
        if (user.Id == currentUser.UserDatabaseId && roles.All(role => role.Name != RoleNames.Admin))
            throw new UserManagementException(400, "You cannot remove your own Admin role. Ask another administrator to make this change.");

        context.UserRoles.RemoveRange(user.UserRoles.Where(userRole => !desiredRoleIds.Contains(userRole.RoleId)));
        context.UserRoles.AddRange(desiredRoleIds.Except(currentUserRoleIds)
            .Select(roleId => new UserRole { UserId = user.Id, RoleId = roleId }));
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new UserManagementException(409, "This user's roles changed. Refresh the page before editing them again.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "PK_UserRoles" })
        {
            throw new UserManagementException(409, "This user's roles changed. Refresh the page before editing them again.");
        }
        await Send.OkAsync(new Response(roles), ct);
    }
}
