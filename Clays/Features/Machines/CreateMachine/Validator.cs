using FastEndpoints;
using FluentValidation;

namespace Clays.Features.Machines.CreateMachine;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.ManufacturerId).GreaterThan(0);
        RuleFor(request => request.Model).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Year).InclusiveBetween(1800, 2200).When(request => request.Year.HasValue);
        RuleFor(request => request.VinNumber).MaximumLength(100);
        RuleFor(request => request.RegistrationNumber).MaximumLength(50);
        RuleFor(request => request.CountryOfImport).MaximumLength(100);
        RuleFor(request => request.PurchaseYear).InclusiveBetween(1800, 2200).When(request => request.PurchaseYear.HasValue);
        RuleFor(request => request.CompanyId).GreaterThan(0).When(request => request.CompanyId.HasValue);
        RuleFor(request => request.Status).Must(status => !status.HasValue || Enum.IsDefined(status.Value));
    }
}
