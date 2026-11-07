namespace Clays.Application.Common.Interfaces;

/// <summary>
/// Provides the authenticated user's identifier from the current request token.
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }
}