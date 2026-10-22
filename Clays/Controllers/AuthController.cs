using System.Security.Claims;
using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clays.Controllers;

/// <summary>Authenticates users with Google and manages their application session.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IConfiguration configuration,
    IApplicationDbContext context) : ControllerBase
{
    /// <summary>Returns the public Google client ID needed by the browser sign-in widget.</summary>
    [AllowAnonymous]
    [HttpGet("config")]
    [ProducesResponseType<AuthConfigurationResponse>(StatusCodes.Status200OK)]
    public ActionResult<AuthConfigurationResponse> GetConfiguration() =>
        Ok(new AuthConfigurationResponse(configuration["Google:ClientId"]));

    /// <summary>Validates a Google ID token and starts an application session.</summary>
    [AllowAnonymous]
    [HttpPost("google")]
    [ProducesResponseType<AuthUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AuthUserResponse>> SignInWithGoogle(
        GoogleSignInRequest request,
        CancellationToken cancellationToken = default)
    {
        var clientId = configuration["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return Problem(
                "Google sign-in is not configured. Set Google:ClientId.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        if (string.IsNullOrWhiteSpace(request.Credential))
        {
            return BadRequest("A Google ID token is required.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.Credential,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized("The Google credential is invalid or expired.");
        }

        if (string.IsNullOrWhiteSpace(payload.Subject)
            || string.IsNullOrWhiteSpace(payload.Email)
            || !payload.EmailVerified)
        {
            return Unauthorized("Google must provide a verified email address.");
        }

        var isAdmin = IsAdministrator(payload.Email);

        var user = await context.Users
            .SingleOrDefaultAsync(item => item.GoogleSubject == payload.Subject, cancellationToken);

        if (user is null)
        {
            var names = (payload.Name ?? string.Empty).Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            user = new User
            {
                GoogleSubject = payload.Subject,
                Email = payload.Email,
                FirstName = payload.GivenName ?? names.FirstOrDefault(),
                LastName = payload.FamilyName ?? (names.Length > 1 ? names[1] : null),
                IsActive = isAdmin,
                ChatId = 0
            };
            context.Users.Add(user);
        }
        else
        {
            user.Email = payload.Email;
            user.FirstName = payload.GivenName ?? user.FirstName;
            user.LastName = payload.FamilyName ?? user.LastName;
            if (isAdmin)
            {
                user.IsActive = true;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        if (!user.IsActive)
        {
            return Unauthorized("Your account is inactive. Contact an administrator to request access.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, payload.Name ?? user.Email!),
            new(ClaimTypes.Email, user.Email!)
        };
        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Ok(ToResponse(user, isAdmin));
    }

    /// <summary>Gets the profile associated with the current session.</summary>
    [HttpGet("me")]
    [ProducesResponseType<AuthUserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthUserResponse>> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await context.Users.AsNoTracking()
            .SingleAsync(item => item.Id == userId, cancellationToken);
        return Ok(ToResponse(user, User.IsInRole("Admin")));
    }

    /// <summary>Ends the current application session.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SignOutUser()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    private bool IsAdministrator(string email) =>
        (configuration.GetSection("Google:AdminEmails").Get<string[]>() ?? [])
        .Contains(email, StringComparer.OrdinalIgnoreCase);

    private static AuthUserResponse ToResponse(User user, bool isAdmin) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.UserName,
        user.Email,
        user.IsActive,
        isAdmin);
}

/// <summary>Public authentication configuration used by the browser.</summary>
public sealed record AuthConfigurationResponse(string? GoogleClientId);

/// <summary>Google Identity Services credential payload.</summary>
public sealed record GoogleSignInRequest(string Credential);

/// <summary>Authenticated application user profile.</summary>
public sealed record AuthUserResponse(
    int Id,
    string? FirstName,
    string? LastName,
    string? UserName,
    string? Email,
    bool IsActive,
    bool IsAdmin);