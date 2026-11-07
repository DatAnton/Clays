using FastEndpoints;
using FluentValidation;

namespace Clays.Features.Users.UpdateUser;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.FirstName).MaximumLength(200);
        RuleFor(request => request.LastName).MaximumLength(200);
        RuleFor(request => request.UserName).MaximumLength(200);
    }
}
