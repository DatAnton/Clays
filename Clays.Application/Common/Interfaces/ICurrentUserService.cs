namespace Clays.Application.Common.Interfaces;

/// <summary>
/// Provides identifiers from the current authenticated identity.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    int UserDatabaseId { get; }
}
