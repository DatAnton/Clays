using Clays.Application.Common.Interfaces;
using Clays.Features.Contacts.CreateContact;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Contacts.GetContact;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.EndpointWithoutRequest<Response>
{
    public override void Configure() => Get("/api/contacts/{id:int}");

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var contactId = Route<int>("id");
        var contact = await context.Contacts.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == contactId, cancellationToken);
        if (contact is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        await Send.OkAsync(contact.Adapt<Response>(), cancellationToken);
    }
}
