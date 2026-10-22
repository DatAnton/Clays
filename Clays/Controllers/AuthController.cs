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
                UserName = payload.Name ?? payload.Email,
                ChatId = 0
            };
            context.Users.Add(user);
        }
        else
        {
            user.Email = payload.Email;
            user.FirstName = payload.GivenName ?? user.FirstName;
            user.LastName = payload.FamilyName ?? user.LastName;
            user.UserName = payload.Name ?? user.UserName;
        }

        await context.SaveChangesAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email),
            new(ClaimTypes.Email, user.Email!)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Ok(ToResponse(user));
    }

    /// <summary>Gets the profile associated with the current session.</summary>
    [HttpGet("me")]
    [ProducesResponseType<AuthUserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthUserResponse>> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await context.Users.AsNoTracking()
            .SingleAsync(item => item.Id == userId, cancellationToken);
        return Ok(ToResponse(user));
    }

    /// <summary>Ends the current application session.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SignOutUser()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    private static AuthUserResponse ToResponse(User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.UserName,
        user.Email);
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
    string? Email);