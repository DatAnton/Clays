using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Clays.Services;

namespace Clays.Features.Contacts.CreateContact;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/api/contacts");
        Policies(AdminOrStaffAuthorization.Policy);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        if (!await context.Companies.AnyAsync(company => company.Id == request.CompanyId, cancellationToken))
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }
        if (request.MachineryId.HasValue && !await context.Machinery.AnyAsync(
                machinery => machinery.Id == request.MachineryId.Value && machinery.CompanyId == request.CompanyId,
                cancellationToken))
        {
            AddError(request => request.MachineryId, "Select machinery belonging to the contact's company.");
            ThrowIfAnyErrors();
            return;
        }

        var contact = request.Adapt<Contact>();
        contact.FirstName = request.FirstName?.Trim();
        contact.LastName = request.LastName?.Trim();
        contact.PhoneNumber = request.PhoneNumber?.Trim();
        contact.Email = request.Email?.Trim();
        contact.Comment = request.Comment?.Trim();
        context.Contacts.Add(contact);
        await context.SaveChangesAsync(cancellationToken);
        await Features.Common.EndpointResponse.JsonAsync(HttpContext, contact.Adapt<Response>(), 201, cancellationToken);
    }
}
