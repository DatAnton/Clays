using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Clays.Application.Common.Interfaces;
using Clays.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clays.Services;

public sealed class Auth0JwtBearerEvents(IApplicationDbContext context, IHttpClientFactory httpClientFactory) : JwtBearerEvents
{
    private static readonly object UserNotFoundKey = new();

    public override async Task TokenValidated(TokenValidatedContext tokenContext)
    {
        var principal = tokenContext.Principal;
        var subject = principal?.FindFirstValue("sub")
            ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(subject) || principal?.Identity is not ClaimsIdentity { IsAuthenticated: true } identity)
        {
            tokenContext.Fail("The token must identify an authenticated Auth0 user.");
            return;
        }

        var cancellationToken = tokenContext.HttpContext.RequestAborted;
        var userDatabaseId = await context.Users.AsNoTracking()
            .Where(user => user.Auth0Subject == subject)
            .Select(user => (int?)user.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (userDatabaseId is null)
        {
            userDatabaseId = await LinkByEmailAsync(tokenContext, subject, cancellationToken);
            if (userDatabaseId is null)
            {
                tokenContext.HttpContext.Items[UserNotFoundKey] = true;
                tokenContext.Fail("No registered user matches this Auth0 account.");
                return;
            }
        }

        // Only the application database may supply authorization roles and the local user ID.
        foreach (var claimsIdentity in principal.Identities)
        {
            foreach (var claim in claimsIdentity.Claims.Where(claim => claim.Type == claimsIdentity.RoleClaimType
                || claim.Type == CurrentUserService.UserDatabaseIdClaimType).ToList())
            {
                claimsIdentity.RemoveClaim(claim);
            }
        }

        var roles = await context.UserRoles.AsNoTracking()
            .Where(userRole => userRole.UserId == userDatabaseId.Value)
            .Select(userRole => userRole.Role.Name)
            .ToListAsync(cancellationToken);
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, role, ClaimValueTypes.String,
                CurrentUserService.UserDatabaseIdClaimIssuer));
        }

        identity.AddClaim(new Claim(CurrentUserService.UserDatabaseIdClaimType,
            userDatabaseId.Value.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer32,
            CurrentUserService.UserDatabaseIdClaimIssuer));
    }

    public override async Task Challenge(JwtBearerChallengeContext context)
    {
        var userNotFound = context.HttpContext.Items.ContainsKey(UserNotFoundKey);
        context.HandleResponse();
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.WWWAuthenticate = "Bearer";
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = userNotFound ? "User account not found." : "Sign-in required.",
            Detail = userNotFound
                ? "Ask an administrator to register your email and verify it before signing in."
                : "Your session is missing or has expired. Please sign in again."
        }, cancellationToken: context.HttpContext.RequestAborted);
    }

    private async Task<int?> LinkByEmailAsync(TokenValidatedContext tokenContext, string subject,
        CancellationToken cancellationToken)
    {
        // Access tokens need not contain email, so obtain the profile directly from Auth0.
        if (!AuthenticationHeaderValue.TryParse(tokenContext.Request.Headers.Authorization, out var authorization)
            || !string.Equals(authorization.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(authorization.Parameter))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "userinfo");
        request.Headers.Authorization = authorization;
        using var response = await httpClientFactory.CreateClient(Auth0HttpClientNames.UserInfo)
            .SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();

        var profile = await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken);
        if (profile?.Subject != subject || profile.EmailVerified != true || string.IsNullOrWhiteSpace(profile.Email))
        {
            return null;
        }

        var email = profile.Email.Trim().ToLowerInvariant();
        var user = await context.Users.AsNoTracking()
            .Where(user => user.Email == email)
            .Select(user => new { user.Id, user.Auth0Subject })
            .SingleOrDefaultAsync(cancellationToken);
        if (user is null || (user.Auth0Subject is not null && user.Auth0Subject != subject))
        {
            return null;
        }

        // A conditional update prevents another Auth0 account from claiming the same row.
        var updated = await context.Users
            .Where(item => item.Id == user.Id && item.Auth0Subject == null)
            .ExecuteUpdateAsync(update => update.SetProperty(item => item.Auth0Subject, subject), cancellationToken);
        if (updated == 1)
        {
            return user.Id;
        }

        return await context.Users.AsNoTracking()
            .Where(item => item.Auth0Subject == subject)
            .Select(item => (int?)item.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private sealed record UserInfo(
        [property: JsonPropertyName("sub")] string? Subject,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("email_verified")] bool EmailVerified);
}
