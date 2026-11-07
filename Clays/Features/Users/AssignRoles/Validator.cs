using FastEndpoints;
using FluentValidation;

namespace Clays.Features.Users.AssignRoles;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.RoleIds).NotNull().Must(ids => ids is { Length: <= 100 });
        RuleForEach(request => request.RoleIds).GreaterThan(0);
        RuleFor(request => request.ExpectedRoleIds).NotNull().Must(ids => ids is { Length: <= 100 });
        RuleForEach(request => request.ExpectedRoleIds).GreaterThan(0);
    }
}

