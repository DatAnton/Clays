namespace Clays.Features.Users.AssignRoles;

public sealed class Request
{
    public int Id { get; set; }
    public int[] RoleIds { get; set; } = [];
    public int[] ExpectedRoleIds { get; set; } = [];
}

