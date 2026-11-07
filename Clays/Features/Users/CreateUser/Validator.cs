using FastEndpoints;
using FluentValidation;

namespace Clays.Features.Users.CreateUser;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(200);
    }
}
