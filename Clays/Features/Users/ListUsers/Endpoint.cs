using System.Globalization;
using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Clays.Services;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Users.ListUsers;

public sealed class Endpoint(IApplicationDbContext context)
    : FastEndpoints.Endpoint<Request, Response>
{
    private const int PageSize = 25;

    public override void Configure()
    {
        Get("/api/users");
        Policies(AdminAuthorization.Policy);
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        var query = FilterUsers(request.Search);
        var page = Math.Max(1, request.Page);
        var totalCount = await query.CountAsync(ct);
        var offset = (int)Math.Min(((long)page - 1) * PageSize, int.MaxValue);
        var users = await query.OrderBy(user => user.Email).ThenBy(user => user.Id)
            .Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
            .Skip(offset).Take(PageSize).ToListAsync(ct);

        var items = users.Select(CreateUserItem).ToArray();

        await Send.OkAsync(new Response(items, page, PageSize, totalCount), ct);
    }

    private IQueryable<User> FilterUsers(string? search)
    {
        var query = context.Users.AsNoTracking();
        if (string.IsNullOrWhiteSpace(search)) return query;

        var pattern = $"%{search.Trim()}%";
        return query.Where(user => EF.Functions.ILike(user.Email, pattern)
            || (user.FirstName != null && EF.Functions.ILike(user.FirstName, pattern))
            || (user.LastName != null && EF.Functions.ILike(user.LastName, pattern))
            || (user.UserName != null && EF.Functions.ILike(user.UserName, pattern)));
    }

    private static UserItem CreateUserItem(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName,
            user.UserName, user.ChatId?.ToString(CultureInfo.InvariantCulture), user.Auth0Subject,
            !string.IsNullOrEmpty(user.TCToken), !string.IsNullOrEmpty(user.TCRefreshToken),
            user.UserRoles.OrderBy(userRole => userRole.RoleId)
                .Select(userRole => new RoleItem(userRole.RoleId, userRole.Role.Name)).ToArray());
}
