using FastEndpoints;
using FluentValidation;

namespace Clays.Features.MachineryLibrary.CreateMachineryLibrary;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.ManufacturerId).GreaterThan(0);
        RuleFor(request => request.Model).NotEmpty().MaximumLength(200);
    }
}
