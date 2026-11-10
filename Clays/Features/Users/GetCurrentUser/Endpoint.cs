using Clays.Domain.Constants;

namespace Clays.Features.Users.GetCurrentUser;

public sealed class Endpoint : FastEndpoints.EndpointWithoutRequest<Response>
{
    public override void Configure() => Get("/api/users/me");

    public override async Task HandleAsync(CancellationToken ct) =>
        await Send.OkAsync(new Response(User.IsInRole(RoleNames.Admin), User.IsInRole(RoleNames.Staff)), ct);
}
