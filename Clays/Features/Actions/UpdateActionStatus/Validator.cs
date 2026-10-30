using FastEndpoints;
using FluentValidation;

namespace Clays.Features.Actions.UpdateActionStatus;

public sealed class Validator : Validator<Request>
{
    public Validator() => RuleFor(request => request.Status).Must(Enum.IsDefined);
}
