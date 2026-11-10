using Clays.Application.Common.Interfaces;
using Clays.Features.Contacts.CreateContact;
using Clays.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Contacts.UpdateContact;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure()
    {
        Put("/api/contacts/{id:int}");
        Policies(AdminOrStaffAuthorization.Policy);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var contact = await context.Contacts.SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (contact is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

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

        contact.FirstName = request.FirstName?.Trim();
        contact.LastName = request.LastName?.Trim();
        contact.PhoneNumber = request.PhoneNumber?.Trim();
        contact.Email = request.Email?.Trim();
        contact.CompanyId = request.CompanyId;
        contact.MachineryId = request.MachineryId;
        contact.Comment = request.Comment?.Trim();
        await context.SaveChangesAsync(cancellationToken);

        await Send.OkAsync(contact.Adapt<Response>(), cancellationToken);
    }
}
