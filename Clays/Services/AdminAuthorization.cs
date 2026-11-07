using Clays.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Clays.Services;

public static class AdminAuthorization
{
    public const string Policy = "Admin";
    public static void Configure(AuthorizationOptions options) =>
        options.AddPolicy(Policy, policy => policy.RequireAuthenticatedUser()
            .RequireRole(RoleNames.Admin));
}
