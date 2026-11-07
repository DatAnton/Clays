namespace Clays.Features.Users.UpdateUser;

public sealed class Request
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
