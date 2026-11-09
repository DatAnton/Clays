using FastEndpoints;
using FluentValidation;

namespace Clays.Features.Companies.UpdateCompany;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.BusinessId).MaximumLength(50);
        RuleFor(request => request.City).MaximumLength(200);
        RuleFor(request => request.Address).MaximumLength(500);
        RuleFor(request => request.MainBusinessLine).MaximumLength(500);
        RuleFor(request => request.Note).MaximumLength(4000);
        RuleFor(request => request.Status).Must(status => !status.HasValue || Enum.IsDefined(status.Value));
    }
}
