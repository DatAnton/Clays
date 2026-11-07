using System.Security.Claims;
using System.Globalization;
using Clays.Application.Common.Interfaces;

namespace Clays.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public const string UserDatabaseIdClaimType = "UserDatabaseId";
    public const string UserDatabaseIdClaimIssuer = "Clays";

    public int UserDatabaseId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                throw new InvalidOperationException("An authenticated user is required.");
            }

            var value = user.FindFirst(claim => claim.Type == UserDatabaseIdClaimType
                && claim.Issuer == UserDatabaseIdClaimIssuer)?.Value;
            return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0
                ? id
                : throw new InvalidOperationException("The authenticated identity must contain a valid UserDatabaseId claim.");
        }
    }

    public string? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue("sub")
                ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
