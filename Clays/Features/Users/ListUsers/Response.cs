namespace Clays.Features.Users.ListUsers;

public sealed record Response(IReadOnlyList<UserItem> Items, int Page, int PageSize, int TotalCount);

public sealed record UserItem(int Id, string Email, string? FirstName, string? LastName,
    string? UserName, string? ChatId, string? Auth0Subject,
    bool HasTCToken, bool HasTCRefreshToken, IReadOnlyList<RoleItem> Roles);
