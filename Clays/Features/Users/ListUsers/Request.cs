namespace Clays.Features.Users.ListUsers;

public sealed class Request
{
    public int Page { get; set; } = 1;
    public string? Search { get; set; }
}
