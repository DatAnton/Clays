using System.Security.Claims;
using Clays.Application.Common.Interfaces;

namespace Clays.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
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