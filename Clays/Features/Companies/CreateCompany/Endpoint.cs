using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Clays.Features.Common;
using Clays.Features.Companies.ListCompanies;
using Clays.Services;

namespace Clays.Features.Companies.CreateCompany;

public sealed class Endpoint(IApplicationDbContext context, ICurrentUserService currentUser) : FastEndpoints.Endpoint<Request, CompanyItem>
{
    public override void Configure()
    {
        Post("/api/companies");
        Policies(AdminOrStaffAuthorization.Policy);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var company = new Company
        {
            Name = request.Name.Trim(),
            BusinessId = request.BusinessId?.Trim(),
            City = request.City?.Trim(),
            Address = request.Address?.Trim(),
            MainBusinessLine = request.MainBusinessLine?.Trim(),
            IsOurPartner = request.IsOurPartner,
            CreatedById = currentUser.UserDatabaseId,
        };
        context.Companies.Add(company);
        await context.SaveChangesAsync(cancellationToken);

        await EndpointResponse.JsonAsync(HttpContext, new CompanyItem(
            company.Id, company.Name, company.BusinessId, company.City,
            company.Address, company.MainBusinessLine, company.IsOurPartner), 201, cancellationToken);
    }
}
