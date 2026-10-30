using FastEndpoints;
using FluentValidation;

namespace Clays.Features.Contacts.CreateContact;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.FirstName).MaximumLength(200);
        RuleFor(request => request.LastName).MaximumLength(200);
        RuleFor(request => request.PhoneNumber).MaximumLength(50);
        RuleFor(request => request.Email).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.Email));
        RuleFor(request => request.Email).MaximumLength(320);
        RuleFor(request => request.CompanyId).GreaterThan(0);
    }
}
