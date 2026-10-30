using FastEndpoints;
using FluentValidation;

namespace Clays.Features.Companies.CreateComment;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Text).NotEmpty().MaximumLength(4000);
    }
}
