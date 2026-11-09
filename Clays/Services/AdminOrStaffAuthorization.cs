using Clays.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Clays.Services;

public static class AdminOrStaffAuthorization
{
    public const string Policy = "AdminOrStaff";

    public static void Configure(AuthorizationOptions options) =>
        options.AddPolicy(Policy, policy => policy.RequireAuthenticatedUser()
            .RequireRole(RoleNames.Admin, RoleNames.Staff));
}
