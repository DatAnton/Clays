using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Clays.Features.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Companies.CreateComment;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, Response>
{
    public override void Configure() => Post("/api/companies/{companyId:int}/comments");

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var companyId = Route<int>("companyId");
        if (!await context.Companies.AnyAsync(company => company.Id == companyId, cancellationToken))
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        var author = await context.Users.FirstOrDefaultAsync(cancellationToken);
        if (author is null)
        {
            await Send.UnauthorizedAsync(cancellationToken);
            return;
        }

        var comment = new CompanyComment
        {
            CompanyId = companyId,
            Text = request.Text.Trim(),
            AuthorId = author.Id,
            Author = author,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        context.CompanyComments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        await EndpointResponse.JsonAsync(HttpContext, comment.Adapt<Response>(), 201, cancellationToken);
    }
}
