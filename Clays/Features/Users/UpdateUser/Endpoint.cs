using Clays.Application.Common.Interfaces;
using Clays.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Clays.Features.Users.UpdateUser;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure()
    {
        Put("/api/users/{Id:int}");
        Policies(AdminAuthorization.Policy);
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        var user = await context.Users.SingleOrDefaultAsync(user => user.Id == request.Id, ct);
        if (user is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var email = request.Email.Trim().ToLowerInvariant();
        if (await context.Users.AnyAsync(item => item.Id != user.Id && item.Email == email, ct))
            throw new UserManagementException(409, "A user with this email already exists.");

        user.Email = email;
        user.FirstName = Clean(request.FirstName);
        user.LastName = Clean(request.LastName);
        user.UserName = Clean(request.UserName);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_Users_Email" })
        {
            throw new UserManagementException(409, "A user with this email already exists.");
        }
        await Send.OkAsync(new Response(user.Id), ct);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
