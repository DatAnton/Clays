using Clays.Application.Common.Interfaces;
using Clays.Services;
using Microsoft.EntityFrameworkCore;

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
        user.FirstName = Clean(request.FirstName);
        user.LastName = Clean(request.LastName);
        await context.SaveChangesAsync(ct);
        await Send.OkAsync(new Response(user.Id), ct);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
