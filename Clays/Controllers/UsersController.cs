using Clays.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clays.Controllers;

/// <summary>Provides a searchable directory of application users.</summary>
[ApiController]
[Route("api/users")]
public sealed class UsersController(IApplicationDbContext context) : ControllerBase
{
    private const int PageSize = 100;

    /// <summary>Gets a page of users, optionally filtered by name, username, or email.</summary>
    [HttpGet]
    [ProducesResponseType<UserPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserPageResponse>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(user =>
                (user.FirstName != null && EF.Functions.ILike(user.FirstName, pattern))
                || (user.LastName != null && EF.Functions.ILike(user.LastName, pattern))
                || (user.ChatId != 0 && user.UserName != null && EF.Functions.ILike(user.UserName, pattern))
                || (user.Email != null && EF.Functions.ILike(user.Email, pattern)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (int)Math.Min(((long)page - 1) * PageSize, int.MaxValue);
        var users = await query
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .ThenBy(user => user.Id)
            .Skip(offset)
            .Take(PageSize)
            .Select(user => new UserListItemResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.ChatId == 0 ? null : user.UserName,
                user.Email,
                user.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(new UserPageResponse(users, page, PageSize, totalCount));
    }

    /// <summary>Changes whether a user is allowed to sign in.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/active")]
    [ProducesResponseType<UserListItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserListItemResponse>> UpdateUserActiveStatus(
        int id,
        UpdateUserActiveStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = request.IsActive;
        await context.SaveChangesAsync(cancellationToken);

        return Ok(new UserListItemResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.ChatId == 0 ? null : user.UserName,
            user.Email,
            user.IsActive));
    }
}

/// <summary>A public user profile returned by the user directory.</summary>
public sealed record UserListItemResponse(
    int Id,
    string? FirstName,
    string? LastName,
    string? TelegramNickname,
    string? Email,
    bool IsActive);

/// <summary>Payload for changing a user's sign-in access.</summary>
public sealed record UpdateUserActiveStatusRequest(bool IsActive);

/// <summary>A page of users and pagination metadata.</summary>
public sealed record UserPageResponse(
    IReadOnlyList<UserListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);