using FastEndpoints;
using FluentValidation;

namespace Clays.Features.MachineryLibrary.UpdateMachineryLibrary;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.ManufacturerId).GreaterThan(0);
        RuleFor(request => request.Model).NotEmpty().MaximumLength(200);
    }
}
