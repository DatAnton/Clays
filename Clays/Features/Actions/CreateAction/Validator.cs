using FastEndpoints;
using FluentValidation;

namespace Clays.Features.Actions.CreateAction;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.AssignedUserId).GreaterThan(0).When(request => request.AssignedUserId.HasValue);
        RuleFor(request => request.Type).IsInEnum();
        RuleFor(request => request.Status).IsInEnum();
        RuleFor(request => request.Note).MaximumLength(4000);
        RuleFor(request => request.ActionWeek).InclusiveBetween(1, 53);
        RuleFor(request => request.CompanyId).GreaterThan(0);
        RuleFor(request => request.MachineId).GreaterThan(0).When(request => request.MachineId.HasValue);
    }
}
