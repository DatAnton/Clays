using Clays.Application.Common.Interfaces;
using Clays.Features.Companies.ListCompanies;
using Clays.Services;
using Microsoft.EntityFrameworkCore;

namespace Clays.Features.Companies.UpdateCompany;

public sealed class Endpoint(IApplicationDbContext context) : FastEndpoints.Endpoint<Request, CompanyItem>
{
    public override void Configure()
    {
        Put("/api/companies/{id:int}");
        Policies(AdminOrStaffAuthorization.Policy);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var company = await context.Companies.SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (company is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        company.Name = request.Name.Trim();
        company.BusinessId = request.BusinessId?.Trim();
        company.City = request.City?.Trim();
        company.Address = request.Address?.Trim();
        company.MainBusinessLine = request.MainBusinessLine?.Trim();
        company.IsOurPartner = request.IsOurPartner;
        company.Status = request.Status;
        company.Note = request.Note?.Trim();
        await context.SaveChangesAsync(cancellationToken);

        await Send.OkAsync(new CompanyItem(
            company.Id, company.Name, company.BusinessId, company.City,
            company.Address, company.MainBusinessLine, company.IsOurPartner), cancellationToken);
    }
}
